# Non-Cloud Infrastructure: Migrating from AWS ECS to a Home Server

## Current State Summary

| Component | AWS Service | Monthly Cost (approx) |
|---|---|---|
| Backend (.NET 9 API) | ECS Fargate (512 CPU / 1024 MB) | ~$14.60 |
| Load Balancer | ALB | ~$20.00 |
| NAT Gateway | NAT GW + Elastic IP | ~$34.00 |
| Frontend (Angular SPA) | S3 + CloudFront | ~$1.00 |
| Container Registry | ECR | ~$0.20 |
| Secrets | SSM Parameter Store | Free tier |
| **Total per environment** | | **~$69.80** |
| **Total (staging + production)** | | **~$139.60** |

External services that stay as-is: MySQL (Aiven), Redis (Redis Cloud), Cloudflare DNS.

---

## Approach 1: Docker Compose on Windows (WSL2 Backend)

### Overview

Run the existing Docker containers on the laptop using Docker Desktop with WSL2.
This is the closest match to the current ECS setup — the same Docker image that ECS
runs will run on the laptop. Add an nginx container as reverse proxy and Cloudflare
Tunnel for secure ingress.

### Architecture

```
                              Internet
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │      Cloudflare         │
                    │                          │
                    │  tunnel.qawitherev.com   │
                    │  (proxies to cloudflared)│
                    │                          │
                    │  TLS terminated here     │
                    │  DDoS protection         │
                    └────────────┬────────────┘
                                 │
                    Cloudflare Tunnel (outbound only)
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────┐
│                     Home Network                           │
│  ┌──────────────────────────────────────────────────────┐ │
│  │              Old Windows Laptop                     │ │
│  │                                                      │ │
│  │  ┌────────────────────────────────────────────────┐ │ │
│  │  │            WSL2 VM (Ubuntu 24.04)              │ │ │
│  │  │                                                │ │ │
│  │  │  ┌──────────┐    ┌──────────────────────────┐ │ │ │
│  │  │  │cloudflared│    │     Docker Compose       │ │ │ │
│  │  │  │ (tunnel) │───▶│                          │ │ │ │
│  │  │  └──────────┘    │  ┌────────────────────┐  │ │ │ │
│  │  │                  │  │   nginx:latest     │  │ │ │ │
│  │  │                  │  │   (reverse proxy)  │  │ │ │ │
│  │  │                  │  │                    │  │ │ │ │
│  │  │                  │  │  /api/* ──────────▶│──│─│─│──▶ backend:5163
│  │  │                  │  │  /* ──────────────▶│  │ │ │ │   (.NET 9.0 container)
│  │  │                  │  │       static files │  │ │ │ │
│  │  │                  │  └────────────────────┘  │ │ │ │
│  │  │                  │                          │ │ │ │
│  │  │                  │  ┌────────────────────┐  │ │ │ │
│  │  │                  │  │  backend:latest    │  │ │ │ │
│  │  │                  │  │  (ghcr.io/...      │  │ │ │ │
│  │  │                  │  │   .NET 9.0 API)    │  │ │ │ │
│  │  │                  │  │  port 5163         │  │ │ │ │
│  │  │                  │  │                    │  │ │ │ │
│  │  │                  │  │  Env vars from     │  │ │ │ │
│  │  │                  │  │  local .env file   │  │ │ │ │
│  │  │                  │  └────────────────────┘  │ │ │ │
│  │  │                  └──────────────────────────┘ │ │ │
│  │  └────────────────────────────────────────────────┘ │ │
│  │                                                      │ │
│  │  ┌────────────────────────────────────────────────┐ │ │
│  │  │  Windows Host                                  │ │ │
│  │  │  ┌──────────────────┐  ┌────────────────────┐  │ │ │
│  │  │  │ GitHub Actions   │  │ DDNS Updater       │  │ │ │
│  │  │  │ Self-Hosted      │  │ (Cloudflare API)   │  │ │ │
│  │  │  │ Runner           │  │ updates DNS on     │  │ │ │
│  │  │  │                  │  │ IP change          │  │ │ │
│  │  │  └──────────────────┘  └────────────────────┘  │ │ │
│  │  └────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘

External (unchanged):
  ┌──────────────┐    ┌──────────────┐
  │ MySQL (Aiven)│    │ Redis Cloud  │
  └──────────────┘    └──────────────┘
```

### Step-by-Step Implementation

#### Step 1: Prepare the Laptop

```powershell
# Install WSL2 (if not already)
wsl --install -d Ubuntu-24.04

# Limit WSL2 memory usage (important for old hardware)
# Create %USERPROFILE%\.wslconfig:
notepad $env:USERPROFILE\.wslconfig
```

```ini
# .wslconfig
[wsl2]
memory=2GB
processors=2
swap=2GB
```

```powershell
# Restart WSL
wsl --shutdown
```

#### Step 2: Install Docker in WSL2

```bash
# Inside WSL2 Ubuntu
sudo apt update && sudo apt install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
  https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update && sudo apt install -y docker-ce docker-ce-cli containerd.io \
  docker-buildx-plugin docker-compose-plugin

sudo usermod -aG docker $USER
# Restart WSL2: wsl --shutdown from PowerShell, then reopen
```

#### Step 3: Create the Docker Compose File

```bash
mkdir -p ~/parcel-management && cd ~/parcel-management
```

`~/parcel-management/docker-compose.yml`:

```yaml
services:
  nginx:
    image: nginx:alpine
    container_name: pm-nginx
    restart: unless-stopped
    ports:
      - "127.0.0.1:8080:80"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./frontend/dist:/usr/share/nginx/html:ro
    depends_on:
      - backend
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 5s
      retries: 3

  backend:
    image: ghcr.io/qawitherev/parcel-management-backend:latest
    container_name: pm-backend
    restart: unless-stopped
    ports:
      - "127.0.0.1:5163:5163"
    env_file:
      - .env
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5163/health"]
      interval: 10s
      timeout: 5s
      retries: 3
      start_period: 15s

  watchtower:
    image: containrrr/watchtower
    container_name: pm-watchtower
    restart: unless-stopped
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - ~/.docker/config.json:/config.json:ro
    command: --interval 300 pm-backend pm-nginx
    # Auto-updates containers when new images are pushed
```

`~/parcel-management/nginx/nginx.conf`:

```nginx
events {
    worker_connections 1024;
}

http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=30r/s;

    # Backend upstream
    upstream backend {
        server backend:5163;
    }

    server {
        listen 80;
        server_name _;

        # Frontend static files (Angular SPA)
        root /usr/share/nginx/html;
        index index.html;

        # API proxy
        location /api/ {
            limit_req zone=api burst=20 nodelay;
            proxy_pass http://backend;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection keep-alive;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $http_x_forwarded_for;
            proxy_set_header X-Forwarded-For $http_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_read_timeout 60s;
        }

        # Health check (passthrough)
        location /health {
            proxy_pass http://backend;
        }

        # SPA fallback: serve index.html for unknown routes
        location / {
            try_files $uri $uri/ /index.html;
        }
    }
}
```

`~/parcel-management/.env` (secrets — NOT committed to git):

```bash
# These values copied from AWS SSM Parameter Store
ConnectionStrings__DefaultConnection="Server=mysql-7d076dd-qawitherev-hobby-projects.h.aivencloud.com;Port=22115;Database=parcel-management-system;User=avnadmin;Password=xxx;SslMode=VerifyCA;SslCa=/tmp/ca.pem"
JWTSettings__SecretKey=xxx
JWTSettings__Issuer=ParcelManagement
JWTSettings__Audience=ParcelManagement
JWTSettings__ExpirationMinutes=60
Notification__Email__Password=xxx
Notification__Email__Username=apikey
Notification__Email__SmtpHost=smtp.sendgrid.net
Notification__Email__SmtpPort=587
Notification__Email__FromAddress=noreply@qawitherev.com
RedisSettings__ConnectionString=redis-11664.c100.us-east-1-4.ec2.cloud.redislabs.com:11664,password=xxx,ssl=False
Admin__Email=admin@parcelmanagement.com
Admin__Password=xxx
AllowedOrigins=https://parcel-management.qawitherev.com
DbCACert="-----BEGIN CERTIFICATE-----\n...\n-----END CERTIFICATE-----"
```

#### Step 4: Set Up Cloudflare Tunnel

```bash
# Inside WSL2
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb
sudo dpkg -i cloudflared.deb

# Authenticate (opens browser on Windows host)
cloudflared tunnel login

# Create tunnel
cloudflared tunnel create parcel-management
# This outputs a tunnel ID (e.g., abc123-...) and creates credentials.json

# Configure tunnel
sudo mkdir -p /etc/cloudflared
```

`/etc/cloudflared/config.yml`:

```yaml
tunnel: <tunnel-id-from-create-step>
credentials-file: /home/<user>/.cloudflared/<tunnel-id>.json

ingress:
  # Production
  - hostname: parcel-management.qawitherev.com
    service: http://localhost:8080
  - hostname: api-parcel-management.qawitherev.com
    service: http://localhost:8080

  # Staging
  - hostname: staging.parcel-management.qawitherev.com
    service: http://localhost:8081
  - hostname: api-staging-parcel-management.qawitherev.com
    service: http://localhost:8081

  # Fallback
  - service: http_status:404
```

```bash
# Route DNS through Cloudflare (creates CNAME records automatically)
cloudflared tunnel route dns parcel-management parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management api-parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management staging.parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management api-staging-parcel-management.qawitherev.com

# Install as systemd service
sudo cloudflared service install
sudo systemctl enable --now cloudflared
```

#### Step 5: DDNS for Direct Access (optional backup)

```bash
# Install a DDNS script that updates Cloudflare DNS when public IP changes
# This is a backup in case the tunnel goes down
```

Create `~/ddns-update.sh` (run via cron every 5 minutes):

```bash
#!/bin/bash
# Updates a backup A record pointing directly to home IP
# Requires CF_API_TOKEN and CF_ZONE_ID from Cloudflare

CURRENT_IP=$(curl -s https://api.ipify.org)
RECORD_IP=$(dig +short home.qawitherev.com)

if [ "$CURRENT_IP" != "$RECORD_IP" ]; then
    curl -s -X PUT "https://api.cloudflare.com/client/v4/zones/$CF_ZONE_ID/dns_records/$RECORD_ID" \
        -H "Authorization: Bearer $CF_API_TOKEN" \
        -H "Content-Type: application/json" \
        --data "{\"type\":\"A\",\"name\":\"home.qawitherev.com\",\"content\":\"$CURRENT_IP\",\"ttl\":120}"
fi
```

#### Step 6: Set Up GitHub Actions Self-Hosted Runner

```powershell
# On Windows host (runs as a Windows service)
mkdir C:\actions-runner
cd C:\actions-runner

# Download runner from GitHub repo Settings > Actions > Runners
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/v2.xxx/actions-runner-win-x64-2.xxx.zip -OutFile runner.zip
Expand-Archive runner.zip .

# Configure
.\config.cmd --url https://github.com/qawitherev/parcel-management-system --token <token>
.\svc.cmd install
.\svc.cmd start
```

### CI/CD Changes

The CD workflows (`cd-backend.yml`, `cd-frontend.yml`) change to:

**Backend deployment:**
```yaml
# Instead of: ECR login → docker push → terraform apply (ECS update)
# New flow:
- name: Build Docker image
  run: docker build -t ghcr.io/qawitherev/pm-backend:${{ github.sha }} .

- name: Push to GitHub Container Registry
  run: |
    echo "${{ secrets.GITHUB_TOKEN }}" | docker login ghcr.io -u ${{ github.actor }} --password-stdin
    docker push ghcr.io/qawitherev/pm-backend:${{ github.sha }}
    docker tag ghcr.io/qawitherev/pm-backend:${{ github.sha }} ghcr.io/qawitherev/pm-backend:latest
    docker push ghcr.io/qawitherev/pm-backend:latest

# Watchtower on the laptop auto-pulls and restarts
# OR: SSH into laptop and run docker compose pull && docker compose up -d

- name: Deploy to home server
  run: |
    ssh home-server "cd ~/parcel-management && \
      docker compose pull backend && \
      docker compose up -d backend"
```

**Frontend deployment:**
```yaml
# Instead of: aws s3 sync → cloudfront invalidation
- name: Build Angular app
  run: npm run build -- --configuration=production

- name: Deploy to home server
  run: |
    rsync -avz --delete dist/frontend/browser/ home-server:~/parcel-management/frontend/dist/
    ssh home-server "docker compose restart nginx"
```

### Pros & Cons

**Pros:**
- Same container image as ECS — zero code changes to the backend
- Watchtower enables automatic updates (pull-based, no push access needed)
- Docker Compose is well-documented and easy to manage
- Cloudflare Tunnel gives TLS, DDoS protection, no open ports

**Cons:**
- Docker Desktop on old hardware consumes significant RAM (2GB+ for VM)
- WSL2 + Docker VM = two layers of virtualization overhead
- Image pulls on old hardware will be slow
- Not using the Windows host directly — most of the laptop sits idle

---

## Approach 2: WSL2 Native (No Docker)

### Overview

Install the .NET 9.0 runtime directly in WSL2 and run the backend as a systemd service.
Serve frontend static files from nginx running natively in WSL2. This eliminates
Docker's overhead entirely while keeping the developer experience Linux-native.

### Architecture

```
                              Internet
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │      Cloudflare         │
                    │  Tunnel + DNS           │
                    └────────────┬────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────┐
│                     Home Network                           │
│  ┌──────────────────────────────────────────────────────┐ │
│  │              Old Windows Laptop                     │ │
│  │                                                      │ │
│  │  ┌────────────────────────────────────────────────┐ │ │
│  │  │            WSL2 VM (Ubuntu 24.04)              │ │ │
│  │  │                                                │ │ │
│  │  │  systemd ─────────────────────────┐            │ │ │
│  │  │                                   │            │ │ │
│  │  │  ┌────────────────────────────┐   │            │ │ │
│  │  │  │     nginx (native)         │   │            │ │ │
│  │  │  │     /etc/nginx/sites-      │   │            │ │ │
│  │  │  │     enabled/pm             │   │            │ │ │
│  │  │  │                            │   │            │ │ │
│  │  │  │  /*       → /var/www/pm/   │   │            │ │ │
│  │  │  │  /api/*   → proxy_pass     │───│────────────┼─┐
│  │  │  │             127.0.0.1:5163 │   │            │ │
│  │  │  └────────────────────────────┘   │            │ │
│  │  │                                   │            │ │
│  │  │  ┌────────────────────────────┐   │            │ │
│  │  │  │  pm-api.service            │   │            │ │
│  │  │  │  (systemd unit)            │◄──┘            │ │
│  │  │  │                            │                │ │
│  │  │  │  ExecStart=/usr/bin/dotnet │                │ │
│  │  │  │    /opt/pm/ParcelMgmt.dll  │                │ │
│  │  │  │  WorkingDirectory=/opt/pm  │                │ │
│  │  │  │  User=pm-svc              │                │ │
│  │  │  │  EnvironmentFile=/etc/pm/  │                │ │
│  │  │  │    .env                    │                │ │
│  │  │  │  Restart=always           │                │ │
│  │  │  └────────────────────────────┘                │ │
│  │  │                                                │ │
│  │  │  ┌────────────────────────────┐                │ │
│  │  │  │  cloudflared.service      │                │ │
│  │  │  │  (Cloudflare Tunnel)      │                │ │
│  │  │  └────────────────────────────┘                │ │
│  │  │                                                │ │
│  │  │  ┌────────────────────────────┐                │ │
│  │  │  │  pm-deploy.service        │                │ │
│  │  │  │  (simple HTTP listener    │                │ │
│  │  │  │   for deployment webhook) │                │ │
│  │  │  └────────────────────────────┘                │ │
│  │  └────────────────────────────────────────────────┘ │ │
│  │                                                      │ │
│  │  ┌────────────────────────────────────────────────┐ │ │
│  │  │  Windows Host                                  │ │ │
│  │  │  ┌──────────────────┐  ┌────────────────────┐  │ │ │
│  │  │  │ WSL Auto-Start   │  │ DDNS Updater       │  │ │ │
│  │  │  │ (scheduled task) │  │ (scheduled task)   │  │ │ │
│  │  │  └──────────────────┘  └────────────────────┘  │ │ │
│  │  └────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
```

### Step-by-Step Implementation

#### Step 1: Prepare WSL2

```powershell
# Install WSL2
wsl --install -d Ubuntu-24.04

# Configure memory limits
notepad $env:USERPROFILE\.wslconfig
```

```ini
[wsl2]
memory=2GB
processors=2
swap=2GB

[boot]
systemd=true
```

```powershell
wsl --shutdown
```

#### Step 2: Install .NET Runtime in WSL2

```bash
# Inside WSL2
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update && sudo apt install -y aspnetcore-runtime-9.0 nginx curl

# Verify
dotnet --version
```

#### Step 3: Deploy the Backend

```bash
# Create directory structure
sudo mkdir -p /opt/parcel-management/backend
sudo mkdir -p /opt/parcel-management/frontend
sudo mkdir -p /etc/parcel-management

# Create service user
sudo useradd -r -s /bin/false pm-svc
sudo chown -R pm-svc:pm-svc /opt/parcel-management
```

`/etc/parcel-management/.env`:

```bash
# Same environment variables as the .env in Approach 1
ConnectionStrings__DefaultConnection="Server=mysql-7d076dd-qawitherev-hobby-projects.h.aivencloud.com;..."
JWTSettings__SecretKey=xxx
JWTSettings__Issuer=ParcelManagement
JWTSettings__Audience=ParcelManagement
JWTSettings__ExpirationMinutes=60
# ... (all other env vars)
ASPNETCORE_URLS=http://127.0.0.1:5163
ASPNETCORE_ENVIRONMENT=Production
```

`/etc/systemd/system/pm-api.service`:

```ini
[Unit]
Description=Parcel Management API
After=network.target

[Service]
Type=simple
User=pm-svc
Group=pm-svc
WorkingDirectory=/opt/parcel-management/backend
EnvironmentFile=/etc/parcel-management/.env
ExecStart=/usr/bin/dotnet /opt/parcel-management/backend/ParcelManagement.Api.dll
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier=pm-api

# Security hardening
NoNewPrivileges=yes
ProtectSystem=strict
ProtectHome=yes
ReadWritePaths=/opt/parcel-management
PrivateTmp=yes

[Install]
WantedBy=multi-user.target
```

#### Step 4: Configure Nginx

`/etc/nginx/sites-available/parcel-management`:

```nginx
# Rate limiting
limit_req_zone $binary_remote_addr zone=api:10m rate=30r/s;

server {
    listen 8080;
    server_name _;

    # Gzip compression
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml;

    # Frontend static files
    root /opt/parcel-management/frontend;
    index index.html;

    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # API proxy
    location /api/ {
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

    # Health check passthrough
    location /health {
        proxy_pass http://127.0.0.1:5163;
    }

    # SPA fallback
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/parcel-management /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl reload nginx
```

#### Step 5: Set Up Cloudflare Tunnel

Same as Approach 1 Step 4 — cloudflared runs as a systemd service inside WSL2, proxying to `http://localhost:8080`.

#### Step 6: Deployment Webhook (Simple HTTP Listener)

Instead of SSH, use a simple webhook receiver that triggers `git pull` + service restart:

```bash
# Create deployment script
sudo nano /usr/local/bin/pm-deploy.sh
```

```bash
#!/bin/bash
set -e

# Pull latest from GitHub, build, and restart
LOG_FILE="/var/log/pm-deploy.log"

echo "[$(date)] Deployment triggered" >> "$LOG_FILE"

# Pull backend source
cd /opt/parcel-management/repo
git pull origin main

# Build
cd backend/src
dotnet publish ParcelManagement.Api -c Release -o /opt/parcel-management/backend-new

# Atomic swap
sudo systemctl stop pm-api
rm -rf /opt/parcel-management/backend
mv /opt/parcel-management/backend-new /opt/parcel-management/backend
sudo chown -R pm-svc:pm-svc /opt/parcel-management/backend
sudo systemctl start pm-api

# Update frontend
cd /opt/parcel-management/repo/frontend
npm ci && npm run build -- --configuration=production
rm -rf /opt/parcel-management/frontend/*
cp -r dist/frontend/browser/* /opt/parcel-management/frontend/

echo "[$(date)] Deployment complete" >> "$LOG_FILE"
```

Use a simple Python webhook listener (or use `webhook` package):

```bash
sudo apt install -y webhook
```

`/etc/webhook.conf`:

```json
[
  {
    "id": "pm-deploy",
    "execute-command": "/usr/local/bin/pm-deploy.sh",
    "command-working-directory": "/opt/parcel-management",
    "response-message": "Deploy triggered",
    "trigger-rule": {
      "match": {
        "type": "payload-hash-sha256",
        "secret": "<webhook-secret>",
        "parameter": {
          "source": "header",
          "name": "X-Hub-Signature-256"
        }
      }
    }
  }
]
```

```bash
webhook -hooks /etc/webhook.conf -port 9000 &
```

GitHub Actions then calls: `https://home.qawitherev.com:9000/hooks/pm-deploy`

### CI/CD Changes

**Backend:**
```yaml
# Instead of ECR push + ECS update:
- name: Notify home server
  run: |
    curl -X POST https://home.qawitherev.com:9000/hooks/pm-deploy \
      -H "X-Hub-Signature-256: sha256=$(echo -n '${{ toJson(github) }}' | \
        openssl dgst -sha256 -hmac '${{ secrets.WEBHOOK_SECRET }}')"
```

**Frontend:**
Same webhook triggers a full deploy script that rebuilds both backend and frontend.

### Pros & Cons

**Pros:**
- No Docker overhead — uses 50-70% less RAM than Approach 1
- .NET runs at native speed (no container layer)
- Full control via systemd (logging via journald, automatic restarts)
- Can use `dotnet publish` directly, no Docker image to push
- Minimum moving parts: nginx + .NET + systemd + cloudflared

**Cons:**
- Must install .NET SDK on the laptop (for `dotnet publish` during deploy)
- Build happens on the old laptop — slow on old hardware
- No container isolation (though systemd unit hardening helps)
- OS updates may break .NET runtime compatibility
- Different from ECS prod environment ("it works on my laptop" risk)

---

## Approach 3: Windows Native with IIS

### Overview

Run everything directly on Windows without any Linux VM. The .NET backend runs in IIS
via the ASP.NET Core Hosting Bundle. Frontend static files are served from IIS as well.
This uses only the Windows host with zero virtualization overhead.

### Architecture

```
                              Internet
                                 │
                                 ▼
                    ┌─────────────────────────┐
                    │      Cloudflare         │
                    │  Tunnel + DNS           │
                    └────────────┬────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────┐
│                     Home Network                           │
│  ┌──────────────────────────────────────────────────────┐ │
│  │              Old Windows Laptop                     │ │
│  │                                                      │ │
│  │  ┌────────────────────────────────────────────────┐ │ │
│  │  │              IIS (inetmgr)                     │ │ │
│  │  │                                                │ │ │
│  │  │  Site: parcel-management (port 8080)           │ │ │
│  │  │  ┌──────────────────────────────────────────┐ │ │ │
│  │  │  │  Application Pool: pm-api                │ │ │ │
│  │  │  │  (No managed code)                       │ │ │ │
│  │  │  │  Identity: pm-svc (local user)           │ │ │ │
│  │  │  │                                          │ │ │ │
│  │  │  │  ┌────────────────────────────────────┐  │ │ │ │
│  │  │  │  │  ASP.NET Core Module               │  │ │ │ │
│  │  │  │  │  (aspnetcoremodule.dll)            │  │ │ │ │
│  │  │  │  │                                    │  │ │ │ │
│  │  │  │  │  Process: dotnet PariManagement.dll│  │ │ │ │
│  │  │  │  │  Port: dynamic (IIS-managed)       │  │ │ │ │
│  │  │  │  └────────────────────────────────────┘  │ │ │ │
│  │  │  └──────────────────────────────────────────┘ │ │ │
│  │  │                                                │ │ │
│  │  │  URL Rewrite Rules:                            │ │ │
│  │  │  /api/* → reverse proxy to backend process      │ │ │
│  │  │  /*     → static files from C:\pm\frontend      │ │ │
│  │  │  (SPA fallback: /* → /index.html)               │ │ │
│  │  └────────────────────────────────────────────────┘ │ │
│  │                                                      │ │
│  │  ┌──────────────────┐  ┌──────────────────────────┐ │ │
│  │  │ cloudflared.exe  │  │ GitHub Actions Runner    │ │ │
│  │  │ (Windows service)│  │ (Windows service)        │ │ │
│  │  │                  │  │                          │ │ │
│  │  │ Proxies:         │  │ Listens for workflow     │ │ │
│  │  │ Cloudflare → IIS │  │ jobs, runs deploy.ps1    │ │ │
│  │  └──────────────────┘  └──────────────────────────┘ │ │
│  │                                                      │ │
│  │  ┌────────────────────────────────────────────────┐ │ │
│  │  │  Scheduled Tasks                               │ │ │
│  │  │  ├── DDNS Updater (every 5 min)                │ │ │
│  │  │  ├── IIS Warmup (on boot)                      │ │ │
│  │  │  └── Log Cleanup (weekly)                      │ │ │
│  │  └────────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────┘
```

### Step-by-Step Implementation

#### Step 1: Enable IIS and Required Features

```powershell
# Run as Administrator
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationInit
Enable-WindowsOptionalFeature -Online -FeatureName IIS-URLRewrite

# Restart
Restart-Computer
```

#### Step 2: Install .NET Hosting Bundle

```powershell
# Download and install ASP.NET Core 9.0 Hosting Bundle
# https://dotnet.microsoft.com/en-us/download/dotnet/9.0
# Get the "Hosting Bundle" installer (includes runtime + IIS module)
Invoke-WebRequest -Uri "https://download.visualstudio.microsoft.com/download/pr/xxx/dotnet-hosting-9.0.0-win.exe" -OutFile dotnet-hosting.exe
Start-Process dotnet-hosting.exe -ArgumentList '/quiet /norestart' -Wait
Restart-Computer

# Verify
dotnet --version
# Should show 9.0.x
```

#### Step 3: Deploy the Backend to IIS

```powershell
# Create directory structure
New-Item -Path C:\pm\backend -ItemType Directory -Force
New-Item -Path C:\pm\frontend -ItemType Directory -Force
New-Item -Path C:\pm\logs -ItemType Directory -Force

# Create local user for the app pool
$password = ConvertTo-SecureString "GenerateARandomPasswordHere123!" -AsPlainText -Force
New-LocalUser -Name "pm-svc" -Password $password -PasswordNeverExpires
```

`C:\pm\backend\web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments="ParcelManagement.Api.dll"
                  stdoutLogEnabled="true" stdoutLogFile="C:\pm\logs\stdout.log"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

**Create IIS Site via PowerShell:**

```powershell
Import-Module WebAdministration

# Create app pool
New-WebAppPool -Name "pm-api"
Set-ItemProperty -Path "IIS:\AppPools\pm-api" -Name managedRuntimeVersion -Value ""
Set-ItemProperty -Path "IIS:\AppPools\pm-api" -Name processModel.identityType -Value SpecificUser
Set-ItemProperty -Path "IIS:\AppPools\pm-api" -Name processModel.userName -Value ".\pm-svc"
Set-ItemProperty -Path "IIS:\AppPools\pm-api" -Name processModel.password -Value "GenerateARandomPasswordHere123!"
Set-ItemProperty -Path "IIS:\AppPools\pm-api" -Name startMode -Value "AlwaysRunning"
Set-ItemProperty -Path "IIS:\AppPools\pm-api" -Name recycling.periodicRestart.time -Value "00:00:00"

# Create site
New-Website -Name "parcel-management" `
    -PhysicalPath "C:\pm\frontend" `
    -ApplicationPool "pm-api" `
    -Port 8080

# Add backend as application under /api
New-WebApplication -Name "api" `
    -Site "parcel-management" `
    -PhysicalPath "C:\pm\backend" `
    -ApplicationPool "pm-api"

# Set environment variables at the server level
Add-WebConfiguration -PSPath "MACHINE/WEBROOT/APPHOST" `
    -Filter "appSettings" `
    -Value @{ key="ConnectionStrings__DefaultConnection"; value="Server=..." }
# ... repeat for all env vars (or use a script to read from .env file)
```

#### Step 4: Configure URL Rewrite for SPA

`C:\pm\frontend\web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- Don't rewrite API calls or static files -->
        <rule name="API" stopProcessing="true">
          <match url="^api/(.*)" />
          <action type="None" />
        </rule>
        <rule name="StaticFiles" stopProcessing="true">
          <match url=".*\.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot|json|txt|map)$" />
          <action type="None" />
        </rule>
        <!-- SPA fallback: everything else goes to index.html -->
        <rule name="SPA Fallback">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>

    <!-- Security headers -->
    <httpProtocol>
      <customHeaders>
        <add name="X-Frame-Options" value="DENY" />
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
      </customHeaders>
    </httpProtocol>

    <!-- Caching for static assets -->
    <staticContent>
      <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="30.00:00:00" />
    </staticContent>
  </system.webServer>
</configuration>
```

#### Step 5: Install Cloudflare Tunnel (Windows)

```powershell
# Download cloudflared for Windows
Invoke-WebRequest -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" `
    -OutFile "C:\pm\cloudflared.exe"

# Authenticate (run from CMD, not PowerShell ISE)
C:\pm\cloudflared.exe tunnel login

# Create tunnel
C:\pm\cloudflared.exe tunnel create parcel-management
```

`%USERPROFILE%\.cloudflared\config.yml`:

```yaml
tunnel: <tunnel-id>
credentials-file: C:\Users\<user>\.cloudflared\<tunnel-id>.json

ingress:
  - hostname: parcel-management.qawitherev.com
    service: http://localhost:8080
  - hostname: api-parcel-management.qawitherev.com
    service: http://localhost:8080
  - hostname: staging.parcel-management.qawitherev.com
    service: http://localhost:8081
  - hostname: api-staging-parcel-management.qawitherev.com
    service: http://localhost:8081
  - service: http_status:404
```

```powershell
# Route DNS
C:\pm\cloudflared.exe tunnel route dns parcel-management parcel-management.qawitherev.com
C:\pm\cloudflared.exe tunnel route dns parcel-management api-parcel-management.qawitherev.com
# ... staging too

# Install as Windows service
C:\pm\cloudflared.exe service install
Start-Service cloudflared
```

#### Step 6: Set Up GitHub Actions Self-Hosted Runner

```powershell
# Download and configure runner
mkdir C:\actions-runner
cd C:\actions-runner
Invoke-WebRequest -Uri https://github.com/actions/runner/releases/download/v2.xxx/actions-runner-win-x64-2.xxx.zip -OutFile runner.zip
Expand-Archive runner.zip .
.\config.cmd --url https://github.com/qawitherev/parcel-management-system --token <token> --name home-laptop
.\svc.cmd install
.\svc.cmd start
```

#### Step 7: Deployment Script

`C:\pm\deploy.ps1` (triggered by GitHub Actions self-hosted runner):

```powershell
param(
    [string]$Component = "all"  # "backend", "frontend", or "all"
)

$ErrorActionPreference = "Stop"
$LogFile = "C:\pm\logs\deploy-$(Get-Date -Format 'yyyy-MM-dd-HHmmss').log"

Function Write-Log {
    param($Message)
    $line = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
    Add-Content -Path $LogFile -Value $line
    Write-Host $line
}

Import-Module WebAdministration

if ($Component -in @("backend", "all")) {
    Write-Log "Building backend..."
    Set-Location C:\pm\repo\backend\src
    dotnet publish ParcelManagement.Api -c Release -o C:\pm\backend-new
    Write-Log "Backend build complete."

    Write-Log "Stopping app pool..."
    Stop-WebAppPool -Name "pm-api"

    Write-Log "Swapping backend..."
    Remove-Item -Recurse -Force C:\pm\backend\*
    Copy-Item -Recurse C:\pm\backend-new\* C:\pm\backend\

    Write-Log "Starting app pool..."
    Start-WebAppPool -Name "pm-api"
    Write-Log "Backend deployed."
}

if ($Component -in @("frontend", "all")) {
    Write-Log "Building frontend..."
    Set-Location C:\pm\repo\frontend
    npm ci
    npm run build -- --configuration=production
    Write-Log "Frontend build complete."

    Write-Log "Swapping frontend..."
    Remove-Item -Recurse -Force C:\pm\frontend\*
    Copy-Item -Recurse C:\pm\repo\frontend\dist\frontend\browser\* C:\pm\frontend\

    Write-Log "Frontend deployed."
}

Write-Log "Deployment complete."
```

### CI/CD Changes

With a self-hosted runner, the CI/CD workflows remain almost identical. The key change:
the `runs-on` label changes from `ubuntu-latest` to `self-hosted`:

```yaml
jobs:
  deploy-backend:
    runs-on: [self-hosted, home-laptop]
    # Runner is on the laptop itself, so:
    # - No need to push Docker images
    # - No need for SSH
    # - Build happens directly on the server
    steps:
      - uses: actions/checkout@v4
      - name: Deploy backend
        run: |
          dotnet publish backend/src/ParcelManagement.Api -c Release -o C:\pm\backend-new
          C:\pm\deploy.ps1 -Component backend
```

### Pros & Cons

**Pros:**
- Zero virtualization overhead — all RAM/CPU available to the app
- IIS is a mature, well-tested web server on Windows
- ASP.NET Core in-process hosting in IIS is efficient
- No Linux knowledge needed — everything is Windows-native
- Self-hosted runner means build + deploy happen on the same machine

**Cons:**
- IIS configuration is XML-heavy and harder to version-control
- Windows updates may reboot the machine (can schedule active hours)
- Running builds on old hardware is slow
- IIS has a larger baseline memory footprint than nginx
- Windows license cost (though pre-installed on the laptop)
- Less community tooling for IaC (no Terraform, harder to automate IIS setup)

---

## Comparison Matrix

| Criteria | Approach 1 (Docker/WSL2) | Approach 2 (WSL2 Native) | Approach 3 (Windows IIS) |
|---|---|---|---|
| **RAM usage (idle)** | ~1.5 GB | ~500 MB | ~800 MB |
| **CPU overhead** | Medium (VM + containers) | Low (VM only) | None |
| **Closest to ECS** | Yes (same image) | No (bare metal) | No (IIS host) |
| **Setup complexity** | Medium | Medium | High |
| **Deployment speed** | Fast (pull image) | Slow (compile on device) | Slow (compile on device) |
| **Version control** | docker-compose.yml | systemd units + scripts | PowerShell + XML |
| **Auto-restart on crash** | Docker restart policy | systemd Restart=always | IIS app pool auto-start |
| **TLS termination** | Cloudflare Tunnel | Cloudflare Tunnel | Cloudflare Tunnel |
| **Logging** | Docker logs → journald | journald | Event Viewer + log files |
| **Build location** | GitHub Actions (cloud) | On the laptop | On the laptop |
| **IaC support** | docker-compose (YAML) | Ansible (optional) | DSC / manual |
| **Recommendation for old laptop** | ⭐⭐ | ⭐⭐⭐ | ⭐ |

---

## Shared Setup (All Approaches)

These steps are the same regardless of which approach you choose.

### DNS Migration from Route53 to Cloudflare

1. Export zone file from Route53
2. Import to Cloudflare (or let Cloudflare scan existing records)
3. Update nameservers at your domain registrar to point to Cloudflare
4. Recreate the same A/CNAME records in Cloudflare:
   - `parcel-management.qawitherev.com` → Cloudflare Tunnel
   - `api-parcel-management.qawitherev.com` → Cloudflare Tunnel
   - `staging.parcel-management.qawitherev.com` → Cloudflare Tunnel
   - `api-staging-parcel-management.qawitherev.com` → Cloudflare Tunnel

### TLS Certificates

Cloudflare provides free edge certificates (TLS from user to Cloudflare).
No ACM certificate management needed anymore.
Cloudflare Tunnel encrypts the connection from Cloudflare to the laptop.

### Secrets Management

Options for replacing AWS SSM Parameter Store:

1. **.env file on disk** (simplest) — restrict file permissions to the service user only
2. **Windows Credential Manager** (Approach 3) — store secrets, access via PowerShell
3. **1Password CLI** — `op inject -i .env.template -o .env`
4. **GitHub Secrets + envsubst** — inject during deployment

Recommended for simplicity: `.env` file with restricted permissions (`chmod 600` on WSL2, or NTFS ACLs on Windows).

### CI/CD Summary

All approaches converge on one of two deployment patterns:

**Pattern A — Cloud build, local pull (Approach 1 with Docker):**
```
Git push → GitHub Actions → Build Docker image → Push to GHCR
→ Watchtower on laptop detects new image → Pull + restart container
```

**Pattern B — Self-hosted runner builds on laptop (Approaches 2 & 3):**
```
Git push → GitHub Actions → Self-hosted runner on laptop
→ git pull → dotnet publish / npm build → restart service
```

Choose Pattern A if build speed matters and you want the laptop to only run pre-built artifacts.
Choose Pattern B if you want fewer external dependencies (no container registry).

### Staging Environment

For staging, run a second instance on a different port:

**Approach 1:** Second docker-compose file (`docker-compose.staging.yml`) with different ports
**Approach 2:** Second systemd unit (`pm-api-staging.service`) on port 5164
**Approach 3:** Second IIS site (`parcel-management-staging`) on port 8081

The nginx/IIS configuration proxies based on the hostname to the correct backend port.

### Monitoring & Health Checks

- **Uptime Kuma** (self-hosted, lightweight) — ping `/health` every 60s, alert on Discord/email
- **Windows Task Manager** or **htop in WSL2** — watch resource usage
- **journald / Event Viewer** — check logs on issues
- Cloudflare Analytics provides traffic and threat metrics

### Backup Strategy

- Database (Aiven): managed backups continue as-is
- Source code: GitHub (unchanged)
- Laptop configs (`docker-compose.yml`, systemd units, nginx configs): commit to repo under `agentic-claude-infra/`
- `.env` secrets: back up to a password manager (1Password, Bitwarden)

### Power & Internet Reliability

- **UPS (uninterruptible power supply):** Highly recommended. Prevents data corruption on power loss
- **BIOS setting:** Set laptop to power on when AC is connected (so it auto-boots after power returns)
- **Windows power settings:** Set to never sleep, lid close = do nothing
- **Internet:** Cloudflare Tunnel auto-reconnects when internet returns. If using DDNS, cron job updates within 5 minutes

---

## Estimated Monthly Savings

| Item | AWS Cost (staging+prod) | Home Server Cost |
|---|---|---|
| ECS Fargate (both envs) | ~$14.60 | $0 |
| ALB (both envs) | ~$40.00 | $0 |
| NAT Gateways (both envs) | ~$68.00 | $0 |
| CloudFront/S3 | ~$2.00 | $0 |
| ECR | ~$0.40 | $0 |
| Route53 (hosted zones) | ~$1.00 | $0 |
| Electricity (laptop 24/7, ~30W) | — | ~$2.50/mo |
| Cloudflare (DNS + Tunnel) | — | Free tier |
| **Total** | **~$126.00/mo** | **~$2.50/mo** |

**Annual savings: ~$1,482.00**

Note: Database (Aiven MySQL) and Redis (Redis Cloud) costs are excluded since they remain unchanged in both scenarios.

---

## Final Recommendation

**For an old Windows laptop, Approach 2 (WSL2 Native) is the strongest choice:**

1. It bypasses Docker's RAM overhead (critical on old hardware)
2. It keeps the Linux-native development experience (nginx, systemd, bash scripts are well-documented and version-controllable)
3. The .NET runtime runs at full native speed without container indirection
4. systemd gives robust process supervision (auto-restart, logging, dependency management)
5. Cloudflare Tunnel handles all the hard networking problems (TLS, DDoS, IP hiding, port blocking)

If the laptop is extremely resource-constrained (4GB RAM or less), consider Approach 3 (IIS) to eliminate the WSL2 VM overhead entirely. If you value operational simplicity and keeping the same artifact as production, go with Approach 1 (Docker).

---

## Appendix: Terraform Cleanup

After migration, destroy the AWS resources to stop billing. Keep only what's still needed:

```bash
# Set enable_compute = false in terraform.tfvars for both environments
# (This is already the current state, so nothing to change)

# Resources that will remain destroyed:
# - ECS services + tasks
# - ALB + target groups + listeners
# - NAT Gateways + Elastic IPs
# - API Route53 A records
# - CloudWatch dashboards

# Resources you may still want:
# - S3 + CloudFront (keep as cold standby / rollback option)
# - ECR (keep if using Docker images)
# - VPC/Subnets (can't delete with Terraform managed, leave as-is)
# - SSM Parameters (keep as source of truth for secrets)

# If you want to fully clean up:
cd terraform/environments/staging
terraform destroy -var="enable_compute=false"

cd terraform/environments/production
terraform destroy -var="enable_compute=false"
```

**Warning:** Full `terraform destroy` will delete:
- S3 buckets (and all frontend files)
- CloudFront distributions
- ECR repositories (and all images)
- SSM parameters (your secrets!)
- VPCs and all networking

Export SSM parameter values before destroying, and consider keeping S3 + CloudFront as a cheap (~$2/mo) rollback option.
