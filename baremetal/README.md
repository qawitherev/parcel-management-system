# Baremetal Deployment — Parcel Management System

Self-hosted deployment on Ubuntu 24.04. No Docker, no Kubernetes, no AWS compute — the app
runs directly on the metal behind an nginx reverse proxy and a Cloudflare Tunnel.

## Architecture

```
Internet
   │
   ▼
Cloudflare (TLS, DDoS, DNS)
   │  api-parcel-management.qawitherev.com  → localhost:8080
   │  api-staging-parcel-management.qawitherev.com → localhost:8081
   │
   ▼
Cloudflare Tunnel (cloudflared — outbound-only, no open ports)
   │
   ▼
┌─────────────────────────────────────────────────────────┐
│  Ubuntu 24.04 Server                                    │
│                                                         │
│  nginx reverse proxy                                    │
│    :8080 → 127.0.0.1:5163   (production)                │
│    :8081 → 127.0.0.1:5164   (staging)                   │
│                                                         │
│  systemd services                                       │
│    pm-api.service          → /opt/pm/backend             │
│    pm-api-staging.service  → /opt/pm/staging-backend    │
│                                                         │
│  Self-hosted GitHub Actions runner (label: baremetal)   │
│    Builds and deploys code on push                      │
└─────────────────────────────────────────────────────────┘

External (unchanged):
  MySQL (Aiven)    Redis Cloud    AWS SSM Parameter Store
```

## Prerequisites

- Ubuntu 24.04 (fresh install or existing)
- Root or sudo access
- Cloudflare Tunnel already created in Cloudflare dashboard (token and credentials JSON)
- AWS credentials with `ssm:GetParametersByPath` on `/{environment}/backend/*` (for .env population)

## Quick Start

### 1. Bootstrap the server

```bash
# Full setup (production + staging, no GitHub runner)
curl -sSL https://raw.githubusercontent.com/qawitherev/parcel-management-system/main/baremetal/bootstrap-ubuntu.sh | sudo bash

# With GitHub Actions runner (one command)
curl -sSL https://raw.githubusercontent.com/qawitherev/parcel-management-system/main/baremetal/bootstrap-ubuntu.sh | sudo GITHUB_RUNNER_TOKEN=<token> bash

# Production only
curl -sSL https://raw.githubusercontent.com/.../bootstrap-ubuntu.sh | sudo ENVIRONMENT=production bash
```

### 2. Populate environment variables

The bootstrap creates placeholder `.env` files. Populate them from AWS SSM:

```bash
# Production
aws ssm get-parameters-by-path \
  --path "/production/backend/" \
  --with-decryption \
  --region ap-southeast-1 \
  --query "Parameters[].[Name,Value]" \
  --output text | while read name value; do
    echo "$(basename "$name")=$value"
done | sudo tee /etc/parcel-management/.env > /dev/null

sudo chmod 600 /etc/parcel-management/.env

# Staging
aws ssm get-parameters-by-path \
  --path "/staging/backend/" \
  --with-decryption \
  --region ap-southeast-1 \
  --query "Parameters[].[Name,Value]" \
  --output text | while read name value; do
    echo "$(basename "$name")=$value"
done | sudo tee /etc/parcel-management/.env.staging > /dev/null

sudo chmod 600 /etc/parcel-management/.env.staging
```

> **Note:** The CD pipeline does this automatically on every deploy. The manual step above
> is only needed for the first deploy — after that the GitHub Actions workflow keeps the
> `.env` files in sync with SSM.

### 3. Deploy the application

Trigger the CD workflow from GitHub Actions:

- **Staging:** Push to the `staging` branch, then run the `CD Backend — Staging` workflow
  with `target: baremetal`
- **Production:** Run the `CD Backend — Production` workflow manually with
  `target: baremetal`

## Bootstrap Script Reference

### What it installs

| Phase | What |
|---|---|
| 1 — Prerequisites | `curl`, `wget`, `gnupg`, `ca-certificates`, `jq`, Microsoft package repo |
| 2 — Runtimes | .NET SDK 9.0, nginx, cloudflared |
| 3 — System Setup | `pm-svc` user, `/opt/pm/` directory tree, `/etc/parcel-management/` |
| 4 — nginx | Site configs for production (`:8080`) and staging (`:8081`) |
| 5 — systemd | `pm-api.service` and `pm-api-staging.service` (hardened) |
| 6 — Environment | Placeholder `.env` files with inline documentation |
| 7 — Runner | GitHub Actions self-hosted runner (only if `GITHUB_RUNNER_TOKEN` is provided) |

### Environment variables

| Variable | Default | Description |
|---|---|---|
| `ENVIRONMENT` | `both` | `production`, `staging`, or `both` |
| `SKIP_DOTNET` | `false` | Skip .NET SDK/runtime installation |
| `SKIP_NGINX` | `false` | Skip nginx installation |
| `SKIP_CLOUDFLARED` | `false` | Skip cloudflared installation |
| `GITHUB_RUNNER_TOKEN` | *(empty)* | If provided, installs and registers the GitHub Actions runner |
| `GITHUB_REPO` | `qawitherev/parcel-management-system` | GitHub repository for the runner |
| `RUNNER_LABELS` | `baremetal,linux-x64` | Labels assigned to the self-hosted runner |

### Idempotency

The script is safe to run multiple times. Every step checks whether its work is already done
and skips if so. Re-running is useful after:

- OS package upgrades
- Verifying configuration integrity after changes
- Adding a new component (e.g., adding staging to a production-only setup)

## File Reference

```
baremetal/
├── bootstrap-ubuntu.sh                  # Self-contained provisioning script
├── README.md                            # This file
├── nginx/
│   ├── parcel-management.conf           # nginx site — production (:8080 → :5163)
│   └── parcel-management-staging.conf   # nginx site — staging (:8081 → :5164)
└── systemd/
    ├── pm-api.service                   # systemd unit — production
    └── pm-api-staging.service           # systemd unit — staging
```

The nginx and systemd files here are **reference copies**. The bootstrap script embeds its
own identical versions as heredocs so it remains a single self-contained file for
`curl | bash` usage.

## Useful Commands

```bash
# Service management
sudo systemctl status pm-api
sudo systemctl status pm-api-staging
sudo systemctl restart pm-api
sudo journalctl -u pm-api -f

# nginx
sudo nginx -t
sudo systemctl reload nginx

# Health checks
curl http://localhost:5163/health
curl http://localhost:5164/health

# Cloudflare Tunnel
sudo systemctl status cloudflared
cloudflared tunnel info

# GitHub Actions runner
sudo systemctl status actions.runner.*
journalctl -u actions.runner.* -f

# Environment files
sudo cat /etc/parcel-management/.env
sudo cat /etc/parcel-management/.env.staging
```

## Adding a New Server

1. Run the bootstrap script on the new server
2. Populate `.env` files from SSM
3. Register a new GitHub Actions runner (with a unique `RUNNER_LABELS` value if needed)
4. Run the CD workflow — it will pick up the new runner via label matching
