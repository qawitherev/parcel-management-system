#!/usr/bin/env bash
# ============================================================
# Parcel Management System — Ubuntu 24.04 Server Bootstrap
# ============================================================
#
# Usage (curl | bash):
#   curl -sSL https://raw.githubusercontent.com/qawitherev/parcel-management-system/main/baremetal/bootstrap-ubuntu.sh | sudo bash
#
# Options (environment variables):
#   ENVIRONMENT            production, staging, or both (default: both)
#   SKIP_DOTNET            skip .NET SDK/runtime install   (default: false)
#   SKIP_NGINX             skip nginx install              (default: false)
#   SKIP_CLOUDFLARED       skip cloudflared install        (default: false)
#   GITHUB_RUNNER_TOKEN    GitHub runner registration token — runner is installed ONLY if provided
#   GITHUB_REPO            GitHub repo for runner           (default: qawitherev/parcel-management-system)
#   RUNNER_LABELS          Comma-separated runner labels    (default: baremetal,linux-x64)
#
# Examples:
#   sudo bash bootstrap-ubuntu.sh                           # provision only (no runner)
#   sudo ENVIRONMENT=production bash bootstrap-ubuntu.sh    # production only
#   sudo GITHUB_RUNNER_TOKEN=ABC123 bash bootstrap-ubuntu.sh  # provision + register runner
# ============================================================

set -euo pipefail

# ── Configuration ──────────────────────────────────────────
ENVIRONMENT="${ENVIRONMENT:-both}"
SKIP_DOTNET="${SKIP_DOTNET:-false}"
SKIP_NGINX="${SKIP_NGINX:-false}"
SKIP_CLOUDFLARED="${SKIP_CLOUDFLARED:-false}"
GITHUB_RUNNER_TOKEN="${GITHUB_RUNNER_TOKEN:-}"
GITHUB_REPO="${GITHUB_REPO:-qawitherev/parcel-management-system}"
RUNNER_LABELS="${RUNNER_LABELS:-baremetal,linux-x64}"
RUNNER_VERSION="${RUNNER_VERSION:-2.323.0}"

PROD_SERVICE="pm-api"
STAGING_SERVICE="pm-api-staging"
PROD_PORT=5163
STAGING_PORT=5164
PROD_DIR="/opt/pm/backend"
STAGING_DIR="/opt/pm/staging-backend"
ENV_DIR="/etc/parcel-management"
SVC_USER="pm-svc"
SVC_GROUP="pm-svc"
ARCH="$(uname -m)"

# ── Helpers ────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
BOLD='\033[1m'
NC='\033[0m'

log()    { echo -e "${GREEN}[✓]${NC} $*"; }
warn()   { echo -e "${YELLOW}[!]${NC} $*"; }
error()  { echo -e "${RED}[✗]${NC} $*" >&2; }
info()   { echo -e "${BLUE}[i]${NC} $*"; }
header() { echo -e "\n${BOLD}═══ $* ═══${NC}\n"; }
skip()   { echo -e "${BLUE}[–]${NC} $* (already present, skipping)"; }

die() {
    error "$*"
    exit 1
}

must_be_root() {
    if [[ "$(id -u)" -ne 0 ]]; then
        die "This script must be run as root. Use: sudo bash bootstrap-ubuntu.sh"
    fi
}

setup_production()  { [[ "$ENVIRONMENT" == "production" || "$ENVIRONMENT" == "both" ]]; }
setup_staging()     { [[ "$ENVIRONMENT" == "staging"    || "$ENVIRONMENT" == "both" ]]; }
setup_runner()      { [[ -n "$GITHUB_RUNNER_TOKEN" ]]; }

# ── Sanity checks ──────────────────────────────────────────
must_be_root

echo -e "${BOLD}"
echo "  ╔══════════════════════════════════════════════════════╗"
echo "  ║   Parcel Management System — Server Bootstrap       ║"
echo "  ╚══════════════════════════════════════════════════════╝"
echo -e "${NC}"
info "Environment:     ${BOLD}${ENVIRONMENT}${NC}"
info "Architecture:    ${ARCH}"
info ".NET SDK skip:   ${SKIP_DOTNET}"
info "nginx skip:      ${SKIP_NGINX}"
info "cloudflared skip: ${SKIP_CLOUDFLARED}"
info "GitHub runner:   $(setup_runner && echo 'yes' || echo 'skip')"
echo ""

# ═════════════════════════════════════════════════════════════
# Phase 1 — Prerequisites
# ═════════════════════════════════════════════════════════════
header "Phase 1: Prerequisites"

log "Updating apt cache..."
apt-get update -qq

log "Installing base packages (curl, wget, gnupg, ca-certificates, jq, unzip)..."
apt-get install -y -qq curl wget gnupg ca-certificates jq unzip

# ── Microsoft package repo (.NET) ──────────────────────────
if [[ "$SKIP_DOTNET" != "true" ]]; then
    if ! dpkg -s packages-microsoft-prod >/dev/null 2>&1; then
        log "Registering Microsoft package repository..."
        wget -q "https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb" \
            -O /tmp/packages-microsoft-prod.deb
        dpkg -i /tmp/packages-microsoft-prod.deb
        rm -f /tmp/packages-microsoft-prod.deb
        apt-get update -qq
    else
        skip "Microsoft package repository"
    fi
fi

# ═════════════════════════════════════════════════════════════
# Phase 2 — Runtimes
# ═════════════════════════════════════════════════════════════
header "Phase 2: Runtime Dependencies"

# ── .NET SDK 9.0 ───────────────────────────────────────────
if [[ "$SKIP_DOTNET" != "true" ]]; then
    if ! command -v dotnet >/dev/null 2>&1; then
        log "Installing .NET SDK 9.0..."
        apt-get install -y -qq dotnet-sdk-9.0
    else
        INSTALLED_DOTNET=$(dotnet --version 2>/dev/null || echo "unknown")
        if [[ "$INSTALLED_DOTNET" == 9.* ]]; then
            skip ".NET SDK ${INSTALLED_DOTNET}"
        else
            warn ".NET SDK ${INSTALLED_DOTNET} found, upgrading to 9.0.x..."
            apt-get install -y -qq dotnet-sdk-9.0
        fi
    fi

    # Verify
    if command -v dotnet >/dev/null 2>&1; then
        log ".NET SDK: $(dotnet --version)"
    else
        die ".NET SDK installation failed"
    fi
fi

# ── nginx ──────────────────────────────────────────────────
if [[ "$SKIP_NGINX" != "true" ]]; then
    if ! command -v nginx >/dev/null 2>&1; then
        log "Installing nginx..."
        apt-get install -y -qq nginx
    else
        skip "nginx $(nginx -v 2>&1 | cut -d'/' -f2)"
    fi

    # Ensure it's enabled and running
    systemctl enable nginx --quiet 2>/dev/null || true
    if ! systemctl is-active --quiet nginx; then
        systemctl start nginx
        log "nginx started"
    fi
fi

# ── cloudflared ────────────────────────────────────────────
if [[ "$SKIP_CLOUDFLARED" != "true" ]]; then
    if ! command -v cloudflared >/dev/null 2>&1; then
        log "Installing cloudflared..."
        curl -sSL https://pkg.cloudflare.com/cloudflare-main.gpg \
            | gpg --dearmor -o /usr/share/keyrings/cloudflare-main.gpg 2>/dev/null
        echo "deb [signed-by=/usr/share/keyrings/cloudflare-main.gpg] https://pkg.cloudflare.com/cloudflared $(lsb_release -cs) main" \
            | tee /etc/apt/sources.list.d/cloudflared.list > /dev/null
        apt-get update -qq
        apt-get install -y -qq cloudflared
    else
        skip "cloudflared $(cloudflared --version 2>/dev/null | head -1 || echo '')"
    fi
fi

# ── AWS CLI v2 ──────────────────────────────────────────
if ! command -v aws >/dev/null 2>&1; then
    log "Installing AWS CLI v2..."
    AWSCLI_ARCH="$(uname -m)"
    case "$AWSCLI_ARCH" in
        x86_64)  AWSCLI_URL_ARCH="x86_64" ;;
        aarch64) AWSCLI_URL_ARCH="aarch64" ;;
        *)       die "Unsupported architecture for AWS CLI: ${AWSCLI_ARCH}" ;;
    esac
    curl -sSL "https://awscli.amazonaws.com/awscli-exe-linux-${AWSCLI_URL_ARCH}.zip" -o /tmp/awscliv2.zip
    unzip -qo /tmp/awscliv2.zip -d /tmp
    /tmp/aws/install --update
    rm -rf /tmp/awscliv2.zip /tmp/aws
else
    skip "AWS CLI ($(aws --version 2>&1 | head -1))"
fi

# ═════════════════════════════════════════════════════════════
# Phase 3 — System Setup
# ═════════════════════════════════════════════════════════════
header "Phase 3: System Setup"

# ── pm-svc user ────────────────────────────────────────────
if ! id "$SVC_USER" >/dev/null 2>&1; then
    log "Creating service user '${SVC_USER}'..."
    useradd --system --user-group --shell /usr/sbin/nologin \
        --home-dir /opt/pm --no-create-home "$SVC_USER"
else
    skip "User '${SVC_USER}'"
fi

# ── Directory structure ────────────────────────────────────
log "Creating directory structure..."
install -d -m 0750 -o "$SVC_USER" -g "$SVC_GROUP" /opt/pm

if setup_production; then
    if [[ -d "$PROD_DIR" ]]; then
        skip "Directory ${PROD_DIR}"
    else
        install -d -m 0750 -o "$SVC_USER" -g "$SVC_GROUP" "$PROD_DIR"
        log "Created ${PROD_DIR}"
    fi
fi

if setup_staging; then
    if [[ -d "$STAGING_DIR" ]]; then
        skip "Directory ${STAGING_DIR}"
    else
        install -d -m 0750 -o "$SVC_USER" -g "$SVC_GROUP" "$STAGING_DIR"
        log "Created ${STAGING_DIR}"
    fi
fi

# ── Environment file directory ─────────────────────────────
if [[ ! -d "$ENV_DIR" ]]; then
    install -d -m 0700 -o root -g root "$ENV_DIR"
    log "Created ${ENV_DIR}"
else
    skip "Directory ${ENV_DIR}"
fi

# ═════════════════════════════════════════════════════════════
# Phase 4 — nginx Site Configs
# ═════════════════════════════════════════════════════════════
header "Phase 4: nginx Configuration"

NGINX_SITES="/etc/nginx/sites-available"
NGINX_ENABLED="/etc/nginx/sites-enabled"

write_nginx_prod() {
    cat <<'NGINX_EOF'
# ============================================================
# Parcel Management API — Production
# Reverse proxy: Cloudflare Tunnel → :8080 → backend :5163
# ============================================================

limit_req_zone $binary_remote_addr zone=api:10m rate=30r/s;

server {
    listen 8080;
    server_name _;

    gzip on;
    gzip_types application/json;

    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    location / {
        limit_req zone=api burst=20 nodelay;
        proxy_pass http://127.0.0.1:5163;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $http_x_forwarded_for;
        proxy_set_header X-Forwarded-For $http_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 60s;
    }
}
NGINX_EOF
}

write_nginx_staging() {
    cat <<'NGINX_EOF'
# ============================================================
# Parcel Management API — Staging
# Reverse proxy: Cloudflare Tunnel → :8081 → backend :5164
# ============================================================

limit_req_zone $binary_remote_addr zone=api_staging:10m rate=10r/s;

server {
    listen 8081;
    server_name _;

    gzip on;
    gzip_types application/json;

    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    location / {
        limit_req zone=api_staging burst=10 nodelay;
        proxy_pass http://127.0.0.1:5164;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $http_x_forwarded_for;
        proxy_set_header X-Forwarded-For $http_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 60s;
    }
}
NGINX_EOF
}

if setup_production; then
    if [[ ! -f "${NGINX_SITES}/parcel-management" ]]; then
        log "Writing nginx site: parcel-management (production → :8080)..."
        write_nginx_prod > "${NGINX_SITES}/parcel-management"
    else
        skip "nginx site: parcel-management"
    fi

    # Enable
    if [[ ! -L "${NGINX_ENABLED}/parcel-management" ]]; then
        ln -sf "${NGINX_SITES}/parcel-management" "${NGINX_ENABLED}/parcel-management"
        log "Enabled nginx site: parcel-management"
    fi
fi

if setup_staging; then
    if [[ ! -f "${NGINX_SITES}/parcel-management-staging" ]]; then
        log "Writing nginx site: parcel-management-staging (staging → :8081)..."
        write_nginx_staging > "${NGINX_SITES}/parcel-management-staging"
    else
        skip "nginx site: parcel-management-staging"
    fi

    # Enable
    if [[ ! -L "${NGINX_ENABLED}/parcel-management-staging" ]]; then
        ln -sf "${NGINX_SITES}/parcel-management-staging" "${NGINX_ENABLED}/parcel-management-staging"
        log "Enabled nginx site: parcel-management-staging"
    fi
fi

# Remove default site if it's still there
if [[ -L "${NGINX_ENABLED}/default" ]]; then
    rm -f "${NGINX_ENABLED}/default"
    log "Removed default nginx site"
fi

# Test and reload
if nginx -t 2>/dev/null; then
    systemctl reload nginx
    log "nginx configuration is valid and reloaded"
else
    die "nginx configuration test failed — check /etc/nginx/sites-available/"
fi

# ═════════════════════════════════════════════════════════════
# Phase 5 — systemd Unit Files
# ═════════════════════════════════════════════════════════════
header "Phase 5: systemd Services"

SYSTEMD_DIR="/etc/systemd/system"

write_systemd_prod() {
    cat <<SYSTEMD_EOF
# ============================================================
# Parcel Management API — Production
# Managed by bootstrap-ubuntu.sh
# ============================================================

[Unit]
Description=Parcel Management API (.NET 9.0)
After=network.target

[Service]
Type=simple
User=${SVC_USER}
Group=${SVC_GROUP}
WorkingDirectory=${PROD_DIR}
EnvironmentFile=${ENV_DIR}/.env
ExecStart=/usr/bin/dotnet ${PROD_DIR}/ParcelManagement.Api.dll
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier=${PROD_SERVICE}

# Hardening
NoNewPrivileges=yes
ProtectSystem=strict
ProtectHome=yes
ReadWritePaths=${PROD_DIR} /tmp
PrivateTmp=yes

[Install]
WantedBy=multi-user.target
SYSTEMD_EOF
}

write_systemd_staging() {
    cat <<SYSTEMD_EOF
# ============================================================
# Parcel Management API — Staging
# Managed by bootstrap-ubuntu.sh
# ============================================================

[Unit]
Description=Parcel Management API — Staging
After=network.target

[Service]
Type=simple
User=${SVC_USER}
Group=${SVC_GROUP}
WorkingDirectory=${STAGING_DIR}
EnvironmentFile=${ENV_DIR}/.env.staging
ExecStart=/usr/bin/dotnet ${STAGING_DIR}/ParcelManagement.Api.dll
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier=${STAGING_SERVICE}

# Hardening
NoNewPrivileges=yes
ProtectSystem=strict
ProtectHome=yes
ReadWritePaths=${STAGING_DIR} /tmp
PrivateTmp=yes

[Install]
WantedBy=multi-user.target
SYSTEMD_EOF
}

if setup_production; then
    if [[ ! -f "${SYSTEMD_DIR}/${PROD_SERVICE}.service" ]]; then
        log "Writing systemd unit: ${PROD_SERVICE}.service..."
        write_systemd_prod > "${SYSTEMD_DIR}/${PROD_SERVICE}.service"
    else
        skip "systemd unit: ${PROD_SERVICE}.service"
    fi
fi

if setup_staging; then
    if [[ ! -f "${SYSTEMD_DIR}/${STAGING_SERVICE}.service" ]]; then
        log "Writing systemd unit: ${STAGING_SERVICE}.service..."
        write_systemd_staging > "${SYSTEMD_DIR}/${STAGING_SERVICE}.service"
    else
        skip "systemd unit: ${STAGING_SERVICE}.service"
    fi
fi

systemctl daemon-reload

# ═════════════════════════════════════════════════════════════
# Phase 6 — Environment Files
# ═════════════════════════════════════════════════════════════
header "Phase 6: Environment Files (.env)"

write_env_placeholder() {
    local env_name="$1"
    local env_path="$2"
    local param_path="$3"

    if [[ -f "$env_path" ]] && [[ -s "$env_path" ]]; then
        skip ".env file: ${env_path} (already populated)"
        return
    fi

    log "Creating placeholder .env: ${env_path}..."
    cat > "$env_path" <<ENV_EOF
# ============================================================
# Parcel Management API — ${env_name} Environment
# ============================================================
#
# This file is populated by the CI/CD pipeline from AWS SSM
# Parameter Store. Do not edit manually.
#
# Source: aws ssm get-parameters-by-path \\
#           --path "/${param_path}/backend/" \\
#           --with-decryption \\
#           --region ap-southeast-1
#
# To populate manually, run:
#   aws ssm get-parameters-by-path \\
#     --path "/${param_path}/backend/" \\
#     --with-decryption \\
#     --region ap-southeast-1 \\
#     --query "Parameters[].[Name,Value]" \\
#     --output text | while read name value; do
#       echo "\$(basename "\$name")=\$value"
#   done | sudo tee ${env_path} > /dev/null
#
# ============================================================
ENV_EOF

    chmod 600 "$env_path"
    chown root:root "$env_path"
}

if setup_production; then
    write_env_placeholder "Production" "${ENV_DIR}/.env" "production"
fi

if setup_staging; then
    write_env_placeholder "Staging" "${ENV_DIR}/.env.staging" "staging"
fi

# ═════════════════════════════════════════════════════════════
# Phase 7 — GitHub Actions Self-Hosted Runner
# ═════════════════════════════════════════════════════════════
if setup_runner; then
    header "Phase 7: GitHub Actions Runner"

    RUNNER_HOME="/opt/actions-runner"

    if [[ -f "${RUNNER_HOME}/.runner" ]]; then
        skip "GitHub Actions runner (already configured at ${RUNNER_HOME})"
    else
        log "Downloading actions-runner v${RUNNER_VERSION} (${ARCH})..."

        case "$ARCH" in
            x86_64)  RUNNER_ARCH="x64" ;;
            aarch64) RUNNER_ARCH="arm64" ;;
            *)       die "Unsupported architecture for GitHub runner: ${ARCH}" ;;
        esac

        RUNNER_TARBALL="actions-runner-linux-${RUNNER_ARCH}-${RUNNER_VERSION}.tar.gz"
        RUNNER_URL="https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/${RUNNER_TARBALL}"

        mkdir -p "$RUNNER_HOME"
        curl -sSL -o "/tmp/${RUNNER_TARBALL}" "$RUNNER_URL"
        tar xzf "/tmp/${RUNNER_TARBALL}" -C "$RUNNER_HOME"
        rm -f "/tmp/${RUNNER_TARBALL}"

        log "Configuring runner for ${GITHUB_REPO}..."
        cd "$RUNNER_HOME"

        # The configure script requires being run as non-root
        # We run it as the user who invoked sudo
        SUDO_USER="${SUDO_USER:-root}"
        if [[ "$SUDO_USER" != "root" ]]; then
            chown -R "${SUDO_USER}:${SUDO_USER}" "$RUNNER_HOME"

            sudo -u "$SUDO_USER" ./config.sh \
                --url "https://github.com/${GITHUB_REPO}" \
                --token "$GITHUB_RUNNER_TOKEN" \
                --name "$(hostname)-${ENVIRONMENT}" \
                --labels "${RUNNER_LABELS}" \
                --unattended \
                --replace

            log "Installing runner as systemd service..."
            ./svc.sh install "$SUDO_USER"
            ./svc.sh start
        else
            warn "No SUDO_USER detected — skipping runner service install."
        fi

        log "GitHub Actions runner installed and running"
    fi
fi

# ═════════════════════════════════════════════════════════════
# Summary
# ═════════════════════════════════════════════════════════════
header "Bootstrap Complete"

echo ""
echo -e "  ${BOLD}Server is provisioned.${NC}"
echo ""

if setup_production; then
    echo -e "  ${GREEN}Production${NC}"
    echo "    Directory:   ${PROD_DIR}"
    echo "    Service:     ${PROD_SERVICE}.service"
    echo "    Env file:    ${ENV_DIR}/.env"
    echo "    Health:      http://localhost:${PROD_PORT}/health"
    echo ""
fi

if setup_staging; then
    echo -e "  ${YELLOW}Staging${NC}"
    echo "    Directory:   ${STAGING_DIR}"
    echo "    Service:     ${STAGING_SERVICE}.service"
    echo "    Env file:    ${ENV_DIR}/.env.staging"
    echo "    Health:      http://localhost:${STAGING_PORT}/health"
    echo ""
fi

echo -e "  ${BOLD}Next steps:${NC}"
echo ""

NEEDS_ENV=false
if setup_production && [[ ! -s "${ENV_DIR}/.env" ]]; then NEEDS_ENV=true; fi
if setup_staging   && [[ ! -s "${ENV_DIR}/.env.staging" ]]; then NEEDS_ENV=true; fi

if $NEEDS_ENV; then
    echo "  1. Populate the .env file(s) from AWS SSM:"
    echo "     # Via GitHub Actions (automatic on next deploy) — OR —"
    echo "     # Manually via the instructions in the .env file"
    echo ""
fi

if setup_production && [[ ! -d "${PROD_DIR}" ]]; then
    echo "  2. Deploy the application via GitHub Actions (cd-backend-baremetal)"
    echo ""
fi

echo "     Useful commands:"
echo "       sudo systemctl status ${PROD_SERVICE}"
echo "       sudo journalctl -u ${PROD_SERVICE} -f"
echo "       sudo systemctl status ${STAGING_SERVICE}"
echo "       sudo journalctl -u ${STAGING_SERVICE} -f"
echo "       sudo nginx -t && sudo systemctl reload nginx"
echo ""
