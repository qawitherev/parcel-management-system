# WSL2 Native Implementation Plan

## Target Audience

You have cloud-native experience (AWS ECS, ALB, S3, Route53, Terraform) but limited
experience with Linux, WSL2, nginx, systemd, or Cloudflare. This guide maps every concept
back to what you already know.

---

## Table of Contents

1. [Concept Mapping: Cloud → Linux](#1-concept-mapping-cloud--linux)
2. [Prerequisites](#2-prerequisites)
3. [Phase 1: WSL2 Setup](#3-phase-1-wsl2-setup)
4. [Phase 2: Install the Stack](#4-phase-2-install-the-stack)
5. [Phase 3: Deploy the Backend](#5-phase-3-deploy-the-backend)
6. [Phase 4: Configure Nginx](#6-phase-4-configure-nginx)
7. [Phase 5: Process Supervision with systemd](#7-phase-5-process-supervision-with-systemd)
8. [Phase 6: Cloudflare Tunnel — How It Works](#8-phase-6-cloudflare-tunnel--how-it-works)
9. [Phase 7: Cloudflare Tunnel — Setup](#9-phase-7-cloudflare-tunnel--setup)
10. [Phase 8: TLS Termination](#10-phase-8-tls-termination)
11. [Phase 9: DNS Migration](#11-phase-9-dns-migration)
12. [Phase 10: CI/CD Pipeline (Dual-Mode)](#12-phase-10-cicd-pipeline-dual-mode)
13. [Phase 11: Staging Environment](#13-phase-11-staging-environment)
14. [Phase 12: Monitoring & Alerts](#14-phase-12-monitoring--alerts)
15. [Phase 13: Maintenance & Day-2 Operations](#15-phase-13-maintenance--day-2-operations)
16. [Troubleshooting Guide](#16-troubleshooting-guide)
17. [Full File Reference](#17-full-file-reference)

---

## 1. Concept Mapping: Cloud → Linux

Before you type a single command, understand what each AWS service maps to:

| AWS Service | What It Does | Linux/WSL2 Equivalent |
|---|---|---|
| **ECS Fargate** | Runs your container, restarts it if it crashes | **systemd service** — a unit file that tells Linux to run your .NET app and restart it if it dies |
| **ALB (Application Load Balancer)** | Receives HTTPS traffic, routes to backend, health checks | **nginx** — listens on a port, proxies `/api/*` to your backend, serves static files for `/*` |
| **ACM (Certificate Manager)** | TLS certificate, auto-renews | **Cloudflare edge certificate** — auto-provisioned, no renewal needed |
| **Route53** | DNS records pointing to ALB/CloudFront | **Cloudflare DNS** — same concept, different UI |
| **CloudFront** | CDN, caches static files at edge | **Cloudflare CDN** — included with Cloudflare DNS (orange cloud = proxied) |
| **SSM Parameter Store** | Stores secrets, injects as env vars | **`.env` file** — plain text file with `KEY=VALUE`, read by systemd |
| **Security Group** | Allows port 443 from 0.0.0.0/0 | **Cloudflare Tunnel** — no open ports needed, Cloudflare reaches IN to your laptop |
| **NAT Gateway** | Lets private subnets reach the internet | **Your home router** — already does NAT, nothing to configure |
| **CloudWatch Logs** | Centralized log storage | **journald** — built into Linux, captures all service stdout/stderr |
| **ECR** | Stores Docker images | **Not needed** — you run the compiled `.dll` directly, no container |
| **Health check (ALB)** | `GET /health` → 200 OK | **nginx health check** or external monitoring via Uptime Kuma |

### Key Networking Difference

```
AWS (what you have now):
  User → CloudFront/S3 (frontend)
  User → ALB (HTTPS) → ECS Fargate (port 5163)

WSL2 Native (what you're building):
  User → Cloudflare (HTTPS + CDN)
       → Cloudflare Tunnel (outbound from laptop to Cloudflare)
       → nginx on laptop (port 8080)
       → .NET backend on laptop (port 5163, localhost only)
```

The critical insight: **nothing reaches your laptop directly.** Cloudflare Tunnel
creates an outbound connection from your laptop to Cloudflare's edge. User traffic
hits Cloudflare, Cloudflare sends it back through that tunnel. Your home IP is hidden
and no ports need to be forwarded.

---

## 2. Prerequisites

### Hardware

- Old Windows laptop (Windows 10 version 2004+ or Windows 11)
- 4GB RAM minimum (2GB allocated to WSL2), 8GB recommended
- 20GB free disk space
- Laptop plugged into power 24/7 (set power settings to never sleep)

### Accounts

- **Cloudflare account** (free tier) — you'll migrate DNS here from Route53
- **GitHub account** — already using this
- Domain: `qawitherev.com` (move from Route53 to Cloudflare)

### Software (on the laptop)

- Windows Terminal (install from Microsoft Store — much better than the default console)
- A code editor (VS Code recommended — it has excellent WSL2 integration)

### Knowledge Prerequisites

This guide assumes you can:
- Open PowerShell as Administrator
- SSH into a server (conceptually — you won't actually SSH here)
- Read YAML/JSON configuration
- Understand what an environment variable is

---

## 3. Phase 1: WSL2 Setup

**Goal:** Get a Linux environment running inside Windows.

### What is WSL2?

Windows Subsystem for Linux 2 runs a real Linux kernel inside a lightweight VM.
Unlike a traditional VM (VirtualBox, VMware), WSL2:
- Starts in under 2 seconds
- Uses only the RAM it needs (dynamically allocates)
- Lets you access Linux files from Windows (`\\wsl$\`) and Windows files from Linux (`/mnt/c/`)
- Integrates with VS Code (`code .` from inside WSL2 opens VS Code on Windows)

Think of it like this: ECS Fargate runs your container in a micro-VM (Firecracker).
WSL2 runs Ubuntu in a similar lightweight VM. Same concept.

### Step 3.1: Install WSL2

Open **PowerShell as Administrator** (right-click Start → Terminal (Admin)):

```powershell
# Install WSL2 with Ubuntu 24.04
wsl --install -d Ubuntu-24.04
```

This command does four things automatically:
1. Enables the Windows Subsystem for Linux feature
2. Enables the Virtual Machine Platform feature
3. Downloads and installs the Linux kernel
4. Downloads and installs Ubuntu 24.04

After it finishes, **restart your computer**.

On first boot after restart, a terminal window will open asking you to create a
Linux username and password. Pick something simple (e.g., username: `abdul`, password: something memorable).

```
Installing, this may take a few minutes...
Please create a default UNIX user account. The username does not need to match your Windows username.
Enter new UNIX username: abdul
New password: ********
Retype new password: ********
```

**Important Linux concept:** When you type your password in Linux, nothing appears on
screen (no dots, no asterisks). This is normal — it's still receiving your input. Type
blindly and press Enter.

### Step 3.2: Limit WSL2 Memory

WSL2 can consume up to 80% of your host RAM by default. On an old laptop, you want
to cap this.

Open PowerShell (not as admin this time) and create a config file:

```powershell
notepad "$env:USERPROFILE\.wslconfig"
```

Paste this:

```ini
[wsl2]
memory=2GB
processors=2
swap=2GB

[boot]
systemd=true
```

What each line means:

| Setting | What It Does |
|---|---|
| `memory=2GB` | Caps the WSL2 VM at 2GB RAM. Your .NET app idles around 150MB, so 2GB is plenty. |
| `processors=2` | Limits WSL2 to 2 CPU cores. Prevents the VM from hogging all cores during builds. |
| `swap=2GB` | If WSL2 runs out of RAM, it can use 2GB of disk as overflow. Slow but prevents crashes. |
| `systemd=true` | **Critical.** Enables systemd (Linux's init system). Without this, you can't run services that auto-start and auto-restart. Think of systemd as the "ECS control plane" for your laptop. |

Apply the settings by restarting WSL2:

```powershell
wsl --shutdown
```

Then open Ubuntu again from the Start menu. (WSL2 boots fresh on the first terminal open.)

### Step 3.3: Update Ubuntu

Open an Ubuntu terminal (from Start menu, or type `wsl` in PowerShell) and run:

```bash
# apt is Ubuntu's package manager. Think of it like npm/nuget but for system software.
# "update" refreshes the list of available packages (doesn't install anything)
sudo apt update

# "upgrade" installs newer versions of everything already installed
# "-y" means "answer yes to all prompts" (no interactive confirmation)
sudo apt upgrade -y
```

**What is `sudo`?** It means "super user do" — run this command as root (the Linux
equivalent of Administrator). You'll be prompted for the password you created in Step 3.1.

### Step 3.4: Understand the Filesystem

```
Windows:  C:\Users\abdul\Documents\
WSL2:     /home/abdul/                  (your Linux home directory)
          /mnt/c/Users/abdul/Documents/  (same Windows folder, accessed from Linux)
```

**Rule of thumb:**
- Store Linux app files (backend, nginx configs) in `/home/abdul/` or `/opt/`
- Access Windows files via `/mnt/c/` only when you need to share files
- **Never** put your project files in `/mnt/c/` and run them from there — it's extremely slow due to cross-filesystem translation. Clone your repo inside WSL2.

### Phase 1 Verification

```bash
# All of these should work without errors:
wsl --version          # (from PowerShell) Should show WSL version 2.x
uname -r               # (from Ubuntu) Should show a Linux kernel version
systemctl --version    # (from Ubuntu) Should show systemd version
free -h                # (from Ubuntu) Should show ~1.9Gi total memory
```

---

## 4. Phase 2: Install the Stack

**Goal:** Install .NET 9.0 runtime, nginx, and cloudflared inside WSL2.

### Step 4.1: Install .NET 9.0 Runtime

Unlike AWS where you use a Docker image with the runtime baked in, you install the
runtime directly on the OS.

```bash
# 1. Add Microsoft's package repository (so apt knows where to download .NET)
#    This is like adding a NuGet source, but for system packages
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# 2. Refresh package list with the new Microsoft repo included
sudo apt update

# 3. Install the ASP.NET Core 9.0 runtime
#    "aspnetcore-runtime-9.0" = production runtime (run apps)
#    If you need to BUILD on this machine too, install "dotnet-sdk-9.0" instead
sudo apt install -y aspnetcore-runtime-9.0
```

The Terraform ECS task definition does this via the Docker image (`mcr.microsoft.com/dotnet/aspnet:9.0`).
Installing the package directly is the same thing, just without the container.

**But wait — you need the SDK to build the app.** Since you'll be pulling source and building
on the laptop (or pushing pre-built binaries), install the SDK too:

```bash
# The SDK includes the runtime, so this replaces the previous install
sudo apt install -y dotnet-sdk-9.0
```

Verify:

```bash
dotnet --version    # Should print 9.0.x
dotnet --list-runtimes    # Shows all installed runtimes
```

### Step 4.2: Install nginx

```bash
sudo apt install -y nginx curl
```

nginx is now running! Check it:

```bash
# "systemctl status" = "what's the state of this service?"
# Like checking the ECS service dashboard for "running tasks: 1/1"
sudo systemctl status nginx
```

You should see `Active: active (running)`. Press `q` to exit the status screen.

**Stop nginx for now** — you don't want it running on the default config:

```bash
sudo systemctl stop nginx
sudo systemctl disable nginx    # Don't auto-start on boot (yet)
```

**nginx concepts mapped to ALB:**

| nginx Concept | ALB Equivalent |
|---|---|
| `server` block | An ALB listener (e.g., port 443) |
| `location /api/` block | A listener rule (path-based routing) |
| `proxy_pass http://127.0.0.1:5163` | Forward to target group |
| `root /var/www/html` | Static file source (S3 bucket equivalent) |
| `listen 8080` | The port nginx binds to (like ALB listening on 443) |

### Step 4.3: Install cloudflared

cloudflared is the Cloudflare Tunnel client. It creates a persistent outbound connection
from your laptop to Cloudflare's edge. Think of it like this:

```
ECS → ALB → Internet (inbound traffic to AWS)
Laptop → cloudflared → Cloudflare → Internet (outbound tunnel from laptop)
```

```bash
# Download the latest cloudflared
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb

# Install it
sudo dpkg -i cloudflared.deb

# Verify
cloudflared --version
```

You won't configure cloudflared yet — that comes in Phase 6 after everything else is running.

### Phase 2 Verification

```bash
dotnet --version           # 9.0.x
nginx -v                   # nginx version: 1.x.x
cloudflared --version      # cloudflared version 20xx.x.x
systemctl --version        # systemd 255
```

---

## 5. Phase 3: Deploy the Backend

**Goal:** Get the .NET API running on the laptop at `http://127.0.0.1:5163/health`.

### Step 5.1: Choose Your Deployment Strategy

You have two options. Pick one before continuing:

**Option A — Build on the laptop (simpler, slower builds):**
Clone the repo in WSL2, run `dotnet publish` on the laptop. Every deploy does a full build.
Recommended if: you want the simplest setup and don't mind 2-4 minute builds.

**Option B — Build in GitHub Actions, download artifacts (faster, more moving parts):**
GitHub Actions builds the app, creates a `.tar.gz` artifact. The laptop downloads and extracts it.
Recommended if: you want faster deployments and the laptop is very slow.

This guide follows **Option A** for simplicity. If you prefer Option B, see the
CI/CD section (Phase 8) for the artifact-based workflow.

### Step 5.2: Clone the Repository

```bash
# Stay in your home directory
cd ~

# Clone the repo. Use HTTPS if you haven't set up SSH keys.
git clone https://github.com/qawitherev/parcel-management-system.git

# Navigate to the backend source
cd ~/parcel-management-system/backend/src
```

### Step 5.3: Build and Publish

```bash
# "dotnet publish" = compile + bundle everything needed to run
# -c Release     = Release configuration (optimized, no debug symbols)
# -o ~/pm/backend = output directory

dotnet publish ParcelManagement.Api -c Release -o ~/pm/backend
```

This is the same thing your Dockerfile does, minus the container:
```
Dockerfile:    dotnet publish → COPY to /app → docker run
WSL2 Native:   dotnet publish → run directly with `dotnet ParcelManagement.Api.dll`
```

After it finishes, verify the output:

```bash
ls ~/pm/backend/
# Should show: ParcelManagement.Api.dll, web.config, appsettings.json, and many more files
```

### Step 5.4: Test the Backend Manually

```bash
# Run the app from your home directory, NOT as root
cd ~/pm/backend

# Set the URL and environment inline
ASPNETCORE_URLS="http://127.0.0.1:5163" \
ASPNETCORE_ENVIRONMENT="Production" \
dotnet ParcelManagement.Api.dll
```

You'll see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://127.0.0.1:5163
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Open a second Ubuntu terminal** (or split your existing one) and test the health endpoint:

```bash
curl http://127.0.0.1:5163/health
# Should return: Healthy (or a 200 status with a JSON response)
```

If this works, press `Ctrl+C` in the first terminal to stop the app. You've confirmed
the app runs. Now you need to make it run as a managed service.

**Note:** The app likely failed because it's missing environment variables (database
connection string, JWT key, etc.). That's expected. You set those up next.

### Step 5.5: Set Up Environment Variables

Create the environment file. This replaces AWS SSM Parameter Store.

```bash
# Create the config directory (owned by root, readable only by root and the service user)
sudo mkdir -p /etc/parcel-management
sudo touch /etc/parcel-management/.env
```

Now edit the file:

```bash
sudo nano /etc/parcel-management/.env
```

(`nano` is a simple terminal text editor. `Ctrl+O` to save, `Ctrl+X` to exit. VS Code
users: you can also use `code` to open VS Code on Windows for WSL2 files.)

Paste all your environment variables into this file:

```bash
# ────────────────────────────────────────────
# Parcel Management API — Environment Variables
# (Formerly in AWS SSM Parameter Store)
# ────────────────────────────────────────────

# Database (Aiven MySQL)
ConnectionStrings__DefaultConnection="Server=mysql-7d076dd-qawitherev-hobby-projects.h.aivencloud.com;Port=22115;Database=parcel-management-system;User=avnadmin;Password=YOUR_AVNEN_ADMIN_PASSWORD;SslMode=VerifyCA;SslCa=/tmp/ca.pem"

# JWT Settings
JWTSettings__SecretKey=YOUR_JWT_SECRET_KEY
JWTSettings__Issuer=ParcelManagement
JWTSettings__Audience=ParcelManagement
JWTSettings__ExpirationMinutes=60

# Notification (SendGrid)
Notification__Email__Password=YOUR_SENDGRID_API_KEY
Notification__Email__Username=apikey
Notification__Email__SmtpHost=smtp.sendgrid.net
Notification__Email__SmtpPort=587
Notification__Email__FromAddress=noreply@qawitherev.com

# Redis (Redis Cloud)
RedisSettings__ConnectionString=YOUR_REDIS_CONNECTION_STRING

# Admin
Admin__Email=admin@parcelmanagement.com
Admin__Password=YOUR_ADMIN_PASSWORD

# CORS
AllowedOrigins=https://parcel-management.qawitherev.com

# ASP.NET Core
ASPNETCORE_URLS=http://127.0.0.1:5163
ASPNETCORE_ENVIRONMENT=Production

# CA Certificate for Aiven MySQL TLS
DbCACert="-----BEGIN CERTIFICATE-----
YOUR_CA_CERTIFICATE_CONTENT_HERE
-----END CERTIFICATE-----"
```

**Where to get these values:**

Run this AWS CLI command to fetch all your SSM parameters (do this BEFORE destroying
anything in AWS, or from your local machine that has AWS credentials):

```powershell
# From PowerShell (or run from inside WSL2 if you have the AWS CLI installed)
aws ssm get-parameters-by-path \
    --path "/production/backend/" \
    --with-decryption \
    --region ap-southeast-1 \
    --query "Parameters[*].[Name,Value]" \
    --output text
```

Replace `production` with `staging` for staging env vars.

**Secure the .env file:**

```bash
# Set ownership to root only
sudo chown root:root /etc/parcel-management/.env

# Only root can read it (600 = owner read+write, nobody else can read)
sudo chmod 600 /etc/parcel-management/.env
```

This is the Linux equivalent of an SSM SecureString — only privileged processes can read it.

### Step 5.6: Create the Service User

ECS runs your container as an isolated user. You'll create a dedicated Linux user for the
same purpose:

```bash
# Create a system user (no login shell, no home directory)
# -r = system user (UID < 1000)
# -s /bin/false = can never log in interactively
sudo useradd -r -s /bin/false pm-svc

# Give this user ownership of the backend files
sudo chown -R pm-svc:pm-svc ~/pm/backend
```

### Step 5.7: Write the CA Certificate

The MySQL connection uses TLS with a CA certificate. Your ECS task injects it via
the `DbCACert` environment variable. The backend code likely writes it to `/tmp/ca.pem`
at startup. However, to be safe, also write it as an actual file:

```bash
# Extract the CA cert from the env var and write to a file
# (Run this after the .env file is created)
source /etc/parcel-management/.env
echo "$DbCACert" | sudo tee /tmp/ca.pem > /dev/null
sudo chmod 644 /tmp/ca.pem
```

---

## 6. Phase 4: Configure Nginx

**Goal:** nginx listens on port 8080, serves frontend files from `/opt/parcel-management/frontend/`, and proxies `/api/*` to the backend.

### Step 6.1: Create the nginx Configuration

nginx configuration works like this:
- `/etc/nginx/nginx.conf` — main config (don't touch this)
- `/etc/nginx/sites-available/` — all possible site configs
- `/etc/nginx/sites-enabled/` — symlinks to configs you want ACTIVE

This is like having Terraform modules: `sites-available/` is your module source,
`sites-enabled/` is the `terraform apply` — it's what's actually live.

Create the site config:

```bash
sudo nano /etc/nginx/sites-available/parcel-management
```

Paste this:

```nginx
# ────────────────────────────────────────────
# Rate limiting zone (shared memory zone for tracking request rates)
# $binary_remote_addr = client IP (4 bytes, very efficient)
# zone=api:10m       = 10MB shared memory for this zone (~160k IPs)
# rate=30r/s         = 30 requests per second per IP
# ────────────────────────────────────────────
limit_req_zone $binary_remote_addr zone=api:10m rate=30r/s;

server {
    # Listen on port 8080 (cloudflared will tunnel to this)
    listen 8080;
    server_name _;

    # ─── Gzip Compression ───────────────────
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml;

    # ─── Frontend Static Files ──────────────
    root /opt/parcel-management/frontend;
    index index.html;

    # ─── Security Headers ───────────────────
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # ─── API Proxy ──────────────────────────
    location /api/ {
        # Apply rate limit
        limit_req zone=api burst=20 nodelay;

        # Forward to backend
        proxy_pass http://127.0.0.1:5163;

        # Pass through request details
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $http_x_forwarded_for;
        proxy_set_header X-Forwarded-For $http_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Timeout after 60 seconds
        proxy_read_timeout 60s;
    }

    # ─── Health Check ───────────────────────
    location /health {
        proxy_pass http://127.0.0.1:5163;
    }

    # ─── SPA Fallback ───────────────────────
    # Any URL that isn't a real file → serve index.html
    # This is what CloudFront's custom error response does in your current setup
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

### Step 6.2: Enable the Site

```bash
# Create a symlink (shortcut) from sites-enabled to sites-available
# This "activates" the config
sudo ln -s /etc/nginx/sites-available/parcel-management /etc/nginx/sites-enabled/

# Remove the default "Welcome to nginx" site
sudo rm /etc/nginx/sites-enabled/default

# Check that the config syntax is valid
sudo nginx -t
# Should print: "syntax is ok" and "test is successful"

# Reload nginx (applies new config without dropping connections)
sudo systemctl reload nginx

# Enable auto-start on boot
sudo systemctl enable nginx
```

### Step 6.3: Create Frontend Directory

```bash
# Create the directory where nginx will look for static files
sudo mkdir -p /opt/parcel-management/frontend

# For now, create a placeholder index.html so you can verify nginx works
echo "<h1>Parcel Management - Coming Soon</h1>" | sudo tee /opt/parcel-management/frontend/index.html
```

### Step 6.4: Test nginx

```bash
# Test that nginx responds
curl http://127.0.0.1:8080/
# Should return: <h1>Parcel Management - Coming Soon</h1>

# Test the API proxy (this will fail until the backend service is running, but nginx should try)
curl http://127.0.0.1:8080/api/health
# Should return: 502 Bad Gateway (nginx is working, but can't reach backend — expected!)
```

---

## 7. Phase 5: Process Supervision with systemd

**Goal:** Make the backend start automatically on boot and restart if it crashes.

### What is systemd?

systemd is the init system for Ubuntu. It manages services (start, stop, restart, logs,
dependencies). Think of it as ECS's control plane:

| systemd Concept | ECS Equivalent |
|---|---|
| Unit file (`.service`) | Task definition |
| `systemctl start pm-api` | `aws ecs run-task` |
| `systemctl enable pm-api` | Create an ECS service (maintain desired count) |
| `Restart=always` | ECS service auto-healing (replace failed tasks) |
| `journalctl -u pm-api` | CloudWatch Logs for a specific log group |
| `After=network.target` | Container dependency ordering |

### Step 7.1: Create the systemd Service Unit

```bash
sudo nano /etc/systemd/system/pm-api.service
```

Paste this:

```ini
[Unit]
Description=Parcel Management API (.NET 9.0)
Documentation=https://github.com/qawitherev/parcel-management-system
After=network.target
# "After=network.target" means: wait until networking is ready before starting

[Service]
# ─── Process Configuration ──────────────────
Type=simple
# "simple" = systemd considers the service started once the process is spawned
# "notify" would be better (.NET supports it) but "simple" works fine

User=pm-svc
Group=pm-svc
# Runs as the dedicated service user, not root (security best practice)

WorkingDirectory=/home/abdul/pm/backend
# Where the .dll files live

EnvironmentFile=/etc/parcel-management/.env
# Load environment variables from this file

ExecStart=/usr/bin/dotnet /home/abdul/pm/backend/ParcelManagement.Api.dll
# The actual command to run. /usr/bin/dotnet is the full path to the dotnet executable.
# Find it yourself with: which dotnet

# ─── Restart Policy ─────────────────────────
Restart=always
# Always restart if the process dies (like ECS service with desired count = 1)
RestartSec=10
# Wait 10 seconds before restarting (prevents crash loops from hammering the system)
# In Docker terms: restart: unless-stopped
# In ECS terms: this is the service scheduler

# ─── Logging ────────────────────────────────
StandardOutput=journal
StandardError=journal
SyslogIdentifier=pm-api
# All console output goes to journald (Linux's built-in log system)
# Equivalent to: CloudWatch Logs with log group /ecs/parcel-management-system

# ─── Security Hardening ─────────────────────
NoNewPrivileges=yes
# Prevents the process from gaining new privileges (like running sudo)
# Similar to: ECS task definition "privileged: false"

ProtectSystem=strict
# Makes the entire filesystem read-only except for explicit exceptions
# Similar to: Docker read-only root filesystem

ProtectHome=yes
# Prevents access to /home directories (the backend doesn't need them)

ReadWritePaths=/home/abdul/pm/backend /tmp
# Even with ProtectSystem=strict, allow writes to these paths
# The backend writes logs? Needs /tmp for the CA cert

PrivateTmp=yes
# Gives the service its own private /tmp directory (isolated from other services)

[Install]
WantedBy=multi-user.target
# "multi-user.target" = the normal boot target (like runlevel 3)
# This means: start this service when the system reaches multi-user mode
# Equivalent to: ECS service with desiredCount=1 (auto-starts and stays running)
```

### Step 7.2: Start the Service

```bash
# Tell systemd to reload its configuration (it scans /etc/systemd/system/ for new files)
sudo systemctl daemon-reload

# Start the service
sudo systemctl start pm-api

# Check if it's running
sudo systemctl status pm-api
# Look for: Active: active (running)
# If it says "failed" or "inactive", check the logs (next step)

# Enable auto-start on boot
sudo systemctl enable pm-api
```

### Step 7.3: Check the Logs

```bash
# View the last 50 lines of logs for the pm-api service
sudo journalctl -u pm-api -n 50 --no-pager

# Follow logs in real-time (like tail -f, or CloudWatch Logs Live Tail)
sudo journalctl -u pm-api -f

# View logs since last boot
sudo journalctl -u pm-api -b

# View logs from a specific time
sudo journalctl -u pm-api --since "2026-05-24 18:00:00"
```

**What to look for in the logs:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://127.0.0.1:5163         ← Good
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.  ← Good

fail: Microsoft.EntityFrameworkCore.Database.Connection[20004]
      An error occurred using the connection to database... ← Bad (check connection string)

Unhandled exception. System.InvalidOperationException... ← Bad (app crashed)
```

### Step 7.4: Verify the Full Chain

```bash
# 1. Verify backend is listening on its port
curl http://127.0.0.1:5163/health
# Should return: Healthy (or 200 OK)
# If not: sudo systemctl status pm-api to see if the service is running

# 2. Verify nginx proxies to the backend
curl http://127.0.0.1:8080/health
# Should return: Healthy (same as above)

# 3. Verify nginx serves the frontend placeholder
curl http://127.0.0.1:8080/
# Should return: <h1>Parcel Management - Coming Soon</h1>

# If all three work, the local stack is healthy.
```

### Step 7.5: Important systemd Commands Cheat Sheet

```bash
sudo systemctl start pm-api       # Start the service
sudo systemctl stop pm-api        # Stop the service
sudo systemctl restart pm-api     # Stop + start (new process)
sudo systemctl reload pm-api      # Reload config without restarting (if the app supports it)
sudo systemctl status pm-api      # Is it running? Any errors?
sudo systemctl enable pm-api      # Auto-start on boot
sudo systemctl disable pm-api     # Don't auto-start on boot
sudo systemctl is-active pm-api   # Prints "active" or "inactive"
sudo systemctl is-enabled pm-api  # Prints "enabled" or "disabled"

# After editing any .service file:
sudo systemctl daemon-reload      # Reload systemd configuration
sudo systemctl restart pm-api     # Apply changes
```

---

## 8. Phase 6: Cloudflare Tunnel — How It Works

**Read this before you run any commands.** Phase 7 is the hands-on setup. This phase
explains what's actually happening so the setup makes sense.

### The Problem Cloudflare Tunnel Solves

In AWS, your ALB has a **public IP address**. DNS points your domain at that IP.
Traffic flows IN:

```
User → DNS lookup → ALB public IP → ECS (private subnet)
```

Your laptop is behind a home router. It has a private IP like `192.168.1.5`.
The internet can't reach it. Traditionally you'd need:

1. A static private IP on the laptop
2. **Port forwarding** on your router (443 → `192.168.1.5:8080`)
3. A **static public IP** from your ISP (or DDNS)
4. A TLS certificate (buy + install + renew)
5. Hope your ISP doesn't block port 80/443 (many do)

Cloudflare Tunnel eliminates ALL of that. How? By reversing the connection direction.

### The Core Idea

Instead of traffic flowing IN to your laptop, your laptop reaches OUT to Cloudflare:

```
Traditional (INBOUND):              Cloudflare Tunnel (OUTBOUND):

User ──→ your public IP ──→         User ──→ Cloudflare edge ──→ your laptop
        your laptop                          (outbound connection
        (router forwards                     initiated by laptop)
         port 443)
```

### Analogy 1: Phone Call vs Walkie-Talkie

**Traditional = Phone call.** Someone needs your number (public IP). Your number must
be listed (DNS). Your phone must ring (open port). If you change numbers or your
carrier blocks the call, nobody reaches you.

**Cloudflare Tunnel = Walkie-talkie.** You and Cloudflare tune to the same channel.
You press the button and say "I'm here." Cloudflare now knows how to reach you.
Users talk to Cloudflare, Cloudflare relays to you through the already-open channel.
No phone number. No port forwarding. You initiated the call, so your router allows
it — just like browsing the web.

### Analogy 2: Hotel Front Desk

```
Route53 DNS = a public directory listing room numbers
Cloudflare DNS = the hotel front desk

Without Cloudflare Tunnel:
  Visitor checks directory → "Room 104" → walks to room 104 → knocks
  But room 104 is your laptop behind a router. There's no door to knock on.

With Cloudflare Tunnel:
  You tell the front desk "I'm in room 104, send my visitors to me"
  Visitor asks front desk → "parcel-management?" → front desk forwards them
  The visitor never sees your room number. No door needs to be open.
```

### How the Connection Works (Moment by Moment)

```
MOMENT 1 — Tunnel starts:
┌──────────┐                                    ┌──────────────┐
│ LAPTOP   │── OUTBOUND connection ────────────▶│  CLOUDFLARE  │
│cloudflared│   "I'm tunnel abc123. Send me     │    EDGE      │
│          │    traffic for these domains..."    │              │
└──────────┘                                    │  Notes:      │
                                                │  abc123 ↔   │
                                                │  parcel-     │
                                                │  management  │
                                                │  .qawitherev │
                                                │  .com        │
                                                └──────────────┘

MOMENT 2 — User visits your site:
┌──────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────┐
│  USER    │────▶│  CLOUDFLARE  │────▶│  CLOUDFLARE  │────▶│ LAPTOP   │
│browser   │     │    DNS       │     │    EDGE      │     │          │
│          │     │  parcel-...  │     │              │     │"Someone  │
│          │     │  → CF IPs    │     │  "I know this│     │ wants    │
│          │     │  (🟠 proxied)│     │   domain.    │     │ parcel-  │
│          │     │              │     │   Tunnel     │     │ ...com!" │
│          │     │              │     │   abc123 is  │     │          │
│          │     │              │     │   waiting."  │     │          │
└──────────┘     └──────────────┘     └──────┬───────┘     └──────────┘
                                             │
                                    Request goes through
                                    the already-open
                                    outbound connection
```

### What Cloudflare Tunnel Replaces from AWS

| AWS Component | What It Did | Cloudflare Tunnel Replacement |
|---|---|---|
| ALB | HTTPS endpoint, routing | Cloudflare edge + nginx on laptop |
| ACM | TLS certificate | Cloudflare auto-provisioned edge cert |
| Security Group (443) | Allow inbound traffic | Not needed — zero inbound ports |
| Public subnet | Where ALB lived | Not needed — laptop is on home LAN |
| NAT Gateway | Private subnet → internet | Home router (always doing NAT) |
| Elastic IP | Static public address | Not needed — IP never exposed |

### Common "Aha" Questions

**"So my laptop calls Cloudflare, not the other way around?"**
Yes. `cloudflared` dials out. Your router sees it as "browsing the web." It has no
idea the traffic is actually serving a website.

**"Do I need port 80/443 open on my router?"**
No. Outbound connections are already allowed (that's how you browse the web). The
tunnel rides on those.

**"What if my ISP changes my public IP?"**
Doesn't matter. The tunnel reconnects within seconds. Cloudflare always knows where
your tunnel is because your laptop called Cloudflare.

**"What if my internet goes down?"**
The tunnel disconnects. Cloudflare shows a 502 page. When internet returns,
`cloudflared` auto-reconnects. No manual intervention.

**"Does this cost money?"**
Cloudflare Tunnel is free. No usage limits, no bandwidth charges. The only
requirement: Cloudflare must be your DNS provider (covered in Phase 9).

**"Can someone find my home IP through this?"**
No. DNS resolves to Cloudflare's IPs, not yours. The tunnel is outbound. Your IP
is never in a DNS record.

---

## 9. Phase 7: Cloudflare Tunnel — Setup

### Step 9.1: Authenticate cloudflared

```bash
# This command opens a browser window on your Windows host
# You'll log into Cloudflare and authorize the tunnel
cloudflared tunnel login
```

What happens:
1. A browser opens on Windows
2. Log into your Cloudflare account (create one at cloudflare.com if you haven't)
3. Select the domain `qawitherev.com` (you must add this domain to Cloudflare first — see Phase 7)
4. Authorize the tunnel

After authorizing, the credential is saved to `~/.cloudflared/cert.pem`.

**If the browser doesn't open** (or you're SSH'd in), the terminal prints a URL.
Copy it and open it manually on your Windows machine.

### Step 9.2: Create the Tunnel

```bash
cloudflared tunnel create parcel-management
```

Output:
```
Tunnel credentials written to /home/abdul/.cloudflared/<UUID>.json
cloudflared chose the name "parcel-management" for this tunnel.
Created tunnel parcel-management with id <UUID>
```

Make a note of the tunnel ID (the UUID). You'll need it in the config file.

### Step 9.3: Configure the Tunnel

```bash
# Create the config directory
sudo mkdir -p /etc/cloudflared

# Create the config file
sudo nano /etc/cloudflared/config.yml
```

Paste this (replace placeholders):

```yaml
# ────────────────────────────────────────────
# Cloudflare Tunnel Configuration
# ────────────────────────────────────────────

# Tunnel ID from the "cloudflared tunnel create" output
tunnel: <YOUR-TUNNEL-UUID>

# Path to the credentials file cloudflared created
credentials-file: /home/abdul/.cloudflared/<YOUR-TUNNEL-UUID>.json
# ⚠️  Replace <YOUR-TUNNEL-UUID> with the actual UUID
# ⚠️  Replace /home/abdul/ with your actual Linux username

# ─── Ingress Rules ──────────────────────────
# These define how traffic is routed.
# Think of them like ALB listener rules (hostname → target group).
# Cloudflare evaluates rules top-to-bottom, first match wins.

ingress:
  # Production frontend
  - hostname: parcel-management.qawitherev.com
    service: http://localhost:8080

  # Production API
  - hostname: api.parcel-management.qawitherev.com
    service: http://localhost:8080

  # Staging frontend
  - hostname: staging.parcel-management.qawitherev.com
    service: http://localhost:8081

  # Staging API
  - hostname: api.staging.parcel-management.qawitherev.com
    service: http://localhost:8081

  # SSH access (for GitHub Actions deployments only — see Phase 10)
  - hostname: ssh.qawitherev.com
    service: ssh://localhost:22

  # Catch-all: reject anything that doesn't match a hostname
  - service: http_status:404
```

**Important:** Both the frontend and API hostnames point to port 8080 because nginx
handles both. nginx routes `/api/*` to the backend and `/*` to static files. The
Cloudflare Tunnel config doesn't need to know about this split.

### Step 9.4: Route DNS Records

This tells Cloudflare: "when someone looks up `parcel-management.qawitherev.com`, send
their traffic through this tunnel."

```bash
cloudflared tunnel route dns parcel-management parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management api.parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management staging.parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management api.staging.parcel-management.qawitherev.com
cloudflared tunnel route dns parcel-management ssh.qawitherev.com
```

Each command creates a CNAME record in Cloudflare DNS pointing `<hostname>` → `<tunnel-id>.cfargotunnel.com`.

**Verify in Cloudflare dashboard:** Go to DNS → Records. You should see four CNAME records
with orange cloud icons (proxied).

### Step 9.5: Install SSH Server (for Deployments)

The laptop needs an SSH server so GitHub Actions can connect during deployments
(via a one-off tunnel — covered in Phase 10).

```bash
sudo apt install -y openssh-server
sudo systemctl enable --now ssh
```

Verify:
```bash
ssh localhost
# Should connect successfully (press Ctrl+D to exit)
```

### Step 9.6: Install cloudflared as a systemd Service

```bash
# cloudflared has a built-in command to install itself as a service
sudo cloudflared service install

# Enable and start it
sudo systemctl enable cloudflared
sudo systemctl start cloudflared

# Check status
sudo systemctl status cloudflared
# Should show: Active: active (running)
```

This creates `/etc/systemd/system/cloudflared.service` automatically and starts the tunnel.

### Step 9.7: Test the Full Internet-to-Laptop Flow

```bash
# Test from inside WSL2 using curl with the Host header (simulates browser request)
curl -H "Host: parcel-management.qawitherev.com" http://localhost:8080/health
# Should return: Healthy

# Now test from your PHONE (turn off WiFi, use cellular data):
# Open browser → https://parcel-management.qawitherev.com/health
# Should return: Healthy
```

**⚠️ Important:** DNS propagation can take a few minutes. If it doesn't work immediately,
wait 5 minutes and try again. Check Cloudflare DNS dashboard to confirm the CNAME records exist.

### Step 9.8: Understand Cloudflare Tunnel Commands

```bash
# View all your tunnels
cloudflared tunnel list

# View detailed info for a tunnel
cloudflared tunnel info parcel-management

# Delete a tunnel (if you need to recreate)
cloudflared tunnel delete parcel-management

# View tunnel logs
sudo journalctl -u cloudflared -f

# Test the config without running the service
cloudflared tunnel ingress validate
```

---

## 10. Phase 8: TLS Termination

**Goal:** Understand where encryption ends in the new setup and why.

### What "TLS Termination" Means

TLS (the padlock in your browser) encrypts traffic between two points. "Termination"
is the point where encryption is **decrypted** — where the lock is opened and the
request becomes plain HTTP.

Every HTTPS request goes through at least one termination point. The question is: where?

### Your Current AWS Setup (One Termination)

```
Browser                    ALB                     ECS (Fargate)
   │                         │                         │
   │── HTTPS ───────────────→│                         │
   │  (encrypted)            │  TLS TERMINATED HERE    │
   │                         │                         │
   │                         │── HTTP ────────────────→│
   │                         │  (plain, inside VPC)    │
```

1. Browser encrypts with the ACM certificate on the ALB
2. ALB decrypts (termination point)
3. ALB forwards as plain HTTP over the private VPC
4. Inside the VPC is trusted — no encryption between ALB and ECS

### Your New Setup (Two Terminations)

```
Browser          Cloudflare Edge          cloudflared          nginx           .NET API
   │                   │                       │                  │                │
   │── HTTPS ─────────→│                       │                  │                │
   │  (encrypted)      │  TLS TERMINATED #1    │                  │                │
   │                   │                       │                  │                │
   │                   │── QUIC/HTTP2 ────────→│                  │                │
   │                   │  (encrypted tunnel)   │  TERMINATED #2   │                │
   │                   │                       │                  │                │
   │                   │                       │── HTTP ─────────→│                │
   │                   │                       │  (plain,         │── HTTP ───────→│
   │                   │                       │   localhost)     │  (plain)       │
```

**TLS Termination #1 — Cloudflare Edge:**
The browser's HTTPS ends at Cloudflare. Cloudflare has a free **edge certificate**
for your domain, auto-provisioned, auto-renewed. Request is decrypted here.

**TLS Termination #2 — cloudflared on your laptop:**
Cloudflare re-encrypts and sends through the tunnel (QUIC/HTTP2). `cloudflared`
decrypts it when it arrives. This second encryption is necessary because the
tunnel crosses the public internet between Cloudflare and your home router.

**Plain HTTP on localhost:**
From `cloudflared` → nginx → .NET, traffic is plain HTTP over `127.0.0.1`.
This is safe because localhost traffic never leaves the machine — it's all
in-memory. If an attacker can sniff your localhost, they already own the laptop.

### Why Not Terminate TLS on the Laptop Instead?

You could. There are three options:

**Option A — TLS terminated at Cloudflare (what this plan uses):**
```
User ──HTTPS──▶ CF Edge ──Tunnel──▶ cloudflared ──HTTP──▶ nginx ──HTTP──▶ .NET
```
Cloudflare decrypts. Tunnel encrypts. cloudflared decrypts. Plain to nginx.

**Option B — TLS terminated at nginx on the laptop:**
```
User ──HTTPS──────────────────────────────────────────▶ nginx ──HTTP──▶ .NET
```
The HTTPS blob travels untouched through CF and the tunnel. nginx does the
termination with your own certificate. Cloudflare never sees plaintext.

**Option C — TLS terminated at .NET Kestrel:**
```
User ──HTTPS───────────────────────────────────────────────────────▶ .NET
```
No nginx. No CF decryption. .NET's web server terminates TLS directly.

### Comparison

| | Option A (CF Edge) | Option B (nginx) | Option C (.NET) |
|---|---|---|---|
| Cloudflare can read traffic | Yes | No | No |
| Cloudflare CDN caches static files | Yes | No | No |
| Cloudflare WAF works | Yes | No | No |
| Certificate management | None (auto) | You (Let's Encrypt) | You (Let's Encrypt) |
| CPU load on laptop | Minimal | TLS handshakes | TLS handshakes |
| Complexity | Minimal | Medium | High |

### Why This Plan Uses Option A

For a parcel management system on old hardware, Option A is the pragmatic choice:

1. **CDN caching is the killer feature.** Your Angular `.js` bundles are large
   (500KB+). Your home upload speed is probably 5-20 Mbps. Letting Cloudflare
   cache and serve static assets from their edge (hundreds of locations worldwide)
   means your laptop only handles API requests — not every static file download.

2. **Zero certificate work.** No certbot, no Let's Encrypt cron jobs, no renewal
   panics. Cloudflare handles it all.

3. **Cloudflare WAF.** Basic protection against common web attacks without any
   configuration on your side.

4. **Cloudflare is trusted.** Millions of sites handle far more sensitive data
   (healthcare, finance) through Cloudflare.

If you later decide you want Option B (end-to-end encryption), the change is
one line in `config.yml` (`http://` → `https://`) plus a Let's Encrypt cert on
nginx. The rest of the setup stays the same.

---

## 11. Phase 9: DNS Migration

**Goal:** Move `qawitherev.com` DNS from AWS Route53 to Cloudflare.

### Why Cloudflare Must Be Your DNS Provider

Cloudflare Tunnel only works when Cloudflare manages your DNS. Here's why:

When `cloudflared tunnel route dns` creates a CNAME record like:
```
parcel-management.qawitherev.com → abc123.cfargotunnel.com
```

That `cfargotunnel.com` target is a **Cloudflare-internal** record. It only
resolves when Cloudflare's nameservers are authoritative for your domain.
Cloudflare needs to be the phonebook to know which tunnel belongs to which domain.

```
IF Cloudflare has DNS (works):
  User → DNS lookup → Cloudflare answers → Cloudflare IP
  User connects → Cloudflare sees domain in its proxy table
  Cloudflare says "this belongs to tunnel abc123"
  Cloudflare forwards through the tunnel
  ✅ Works

IF Route53 has DNS (doesn't work):
  User → DNS lookup → Route53 answers → you'd need to point at CF IPs
  But even if you did, traffic arrives at Cloudflare without context
  Cloudflare says "this domain isn't in my system. I don't proxy it."
  502 error
  ❌ Doesn't work
```

The orange cloud (🟠 proxied) in Cloudflare's DNS dashboard is what wires your
domain to your tunnel. Without Cloudflare as the authoritative DNS, there's no
orange cloud, no association, no routing.

### Understanding the Migration

Right now:
```
Registrar → Route53 nameservers → qawitherev.com DNS records
```

After migration:
```
Registrar → Cloudflare nameservers → qawitherev.com DNS records
```

The registrar doesn't change. The nameserver pointer at the registrar is the
**only thing** that changes. This is like changing your domain's phone book
from Route53 to Cloudflare. Nobody visiting your site will notice the transition
if you replicate all records first.

### Step 11.1: Add Your Domain to Cloudflare

1. Go to [Cloudflare Dashboard](https://dash.cloudflare.com)
2. Click **Add a site** (or **+ Add domain**)
3. Enter `qawitherev.com`
4. Select the **Free** plan
5. Cloudflare scans your existing DNS records from Route53
6. Review the scanned records — make sure all important ones are there
7. Cloudflare gives you two nameservers (e.g., `alex.ns.cloudflare.com` and `jill.ns.cloudflare.com`)

### Step 11.2: Update Nameservers at Your Domain Registrar

1. Log into your domain registrar (where you bought `qawitherev.com` — GoDaddy, Namecheap, AWS, etc.)
2. Find "Nameservers" or "DNS settings"
3. Replace the existing Route53 nameservers with the two Cloudflare nameservers
4. Save

**This is the only "scary" step.** DNS change takes 2-48 hours to propagate globally.
During propagation, some users hit old DNS (Route53), some hit new DNS (Cloudflare).
Both resolve to the same places if you replicated records correctly. **There is no downtime.**

### Step 11.3: Verify Propagation

```bash
# Check what nameservers the world sees for your domain
dig qawitherev.com NS
# or use: https://www.whatsmydns.net/#NS/qawitherev.com

# Once all locations show Cloudflare nameservers, migration is complete
```

### Step 11.4: Clean Up Route53

After propagation is confirmed (wait 48 hours to be safe):

```bash
# Don't delete the hosted zone until you're 100% sure everything works
# Route53 hosted zones cost $0.50/month — cheap insurance for a month

# When ready:
# 1. Delete all record sets (except NS and SOA)
# 2. Delete the hosted zone
# Or let Terraform manage this (add a `terraform destroy` for the DNS module)
```

---

## 12. Phase 10: CI/CD Pipeline (Dual-Mode)

**Goal:** Keep the existing AWS pipeline intact. Add a second deployment path for
the laptop. A single `deployment_target` variable controls which path runs.

### The Dual-Mode Design

```
Git Push or Manual Trigger
        │
        ▼
┌──────────────────────────────────────────────────┐
│              GitHub Actions                      │
│                                                  │
│  Tests (always run, ubuntu-latest)               │
│  ┌────────────────────────────────┐              │
│  │ dotnet test / build verify     │              │
│  └────────────────────────────────┘              │
│           │                                      │
│           ▼                                      │
│  deployment_target = ?                           │
│       │           │                              │
│       ▼           ▼                              │
│  ┌─────────┐ ┌──────────┐                        │
│  │   AWS   │ │  LAPTOP  │                        │
│  │  (ECR   │ │  (cloud  │                        │
│  │   ECS   │ │  build + │                        │
│  │   S3)   │ │  SSH via │                        │
│  │         │ │  tunnel) │                        │
│  └─────────┘ └──────────┘                        │
│     ↑             ↑                              │
│  existing     new path                           │
│  unchanged    added alongside                    │
└──────────────────────────────────────────────────┘
```

**AWS path:** Default. Runs on `git push`. Exactly as it works today — ECR, ECS, S3,
Terraform. Zero changes.

**Laptop path:** Manual trigger only. Builds on GitHub's cloud runner (fast), packages
into a `.tar.gz`, opens a one-off Cloudflare SSH tunnel, `scp`'s the artifact to the
laptop, runs `deploy.sh` to swap and restart services.

### Why Not a Self-Hosted Runner?

The earlier version of this plan recommended installing a GitHub Actions runner on
the laptop itself. The problem: building on old hardware is slow (2-4 minutes for
`dotnet publish`, plus `npm ci` + `ng build` for the frontend). The laptop should
only run the app — not compile it.

Instead, builds happen on GitHub's cloud runners (fast, free for public repos),
and only the final artifact is transferred to the laptop.

### Step 12.1: Create deploy.sh on the Laptop + SSH Key

This script receives a pre-built `.tar.gz`, extracts it, stops the service, swaps
files, starts the service, and verifies health. Builds do NOT happen here.

#### The SSH Key

First, generate an SSH key pair so GitHub Actions can authenticate without a password:

```bash
# On the laptop (WSL2)
ssh-keygen -t ed25519 -f ~/.ssh/github-actions -N "" -C "github-actions-deploy"
```

Add the public key to the laptop's authorized keys:

```bash
cat ~/.ssh/github-actions.pub >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
```

Copy the private key to your clipboard or save it:

```bash
cat ~/.ssh/github-actions
# Copy the output. You'll add this as a GitHub secret in Step 12.2.
```

#### The Deploy Script

```bash
nano ~/deploy.sh
```

```bash
#!/bin/bash
# ────────────────────────────────────────────
# Parcel Management — Deployment Script
# Receives pre-built artifacts from GitHub Actions
# Usage: bash deploy.sh backend
#        bash deploy.sh frontend
#        bash deploy.sh all
# ────────────────────────────────────────────

set -e
set -o pipefail

LOG_FILE="$HOME/deploy.log"
BACKEND_DIR="$HOME/pm/backend"
BACKEND_TAR="$HOME/backend.tar.gz"
EXTRACT_DIR="$HOME/pm/backend-new"
SERVICE="pm-api"
HEALTH_URL="http://127.0.0.1:5163/health"
FRONTEND_DIR="/opt/parcel-management/frontend"
FRONTEND_TAR="$HOME/frontend.tar.gz"

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

deploy_backend() {
    log "=== Backend Deployment ==="

    # 1. Extract
    log "Extracting $BACKEND_TAR..."
    rm -rf "$EXTRACT_DIR"
    mkdir -p "$EXTRACT_DIR"
    tar -xzf "$BACKEND_TAR" -C "$EXTRACT_DIR"
    # The tar was created as: tar -czf backend.tar.gz publish/backend/
    # So files are in publish/backend/ inside the extract directory
    if [ -d "$EXTRACT_DIR/publish/backend" ]; then
        mv "$EXTRACT_DIR/publish/backend/"* "$EXTRACT_DIR/"
        rm -rf "$EXTRACT_DIR/publish"
    fi

    # 2. Stop
    log "Stopping $SERVICE..."
    sudo systemctl stop "$SERVICE"

    # 3. Swap
    log "Swapping old → new..."
    rm -rf "$BACKEND_DIR-old"
    mv "$BACKEND_DIR" "$BACKEND_DIR-old" 2>/dev/null || true
    mv "$EXTRACT_DIR" "$BACKEND_DIR"
    sudo chown -R pm-svc:pm-svc "$BACKEND_DIR"

    # 4. Start
    log "Starting $SERVICE..."
    sudo systemctl start "$SERVICE"
    sleep 5

    # 5. Health check
    if curl -sf "$HEALTH_URL" > /dev/null 2>&1; then
        log "Health check PASSED"
        rm -rf "$BACKEND_DIR-old"
        rm -f "$BACKEND_TAR"
    else
        log "Health check FAILED — rolling back"
        sudo systemctl stop "$SERVICE"
        rm -rf "$BACKEND_DIR"
        mv "$BACKEND_DIR-old" "$BACKEND_DIR"
        sudo chown -R pm-svc:pm-svc "$BACKEND_DIR"
        sudo systemctl start "$SERVICE"
        log "Rolled back to previous version"
        exit 1
    fi

    log "=== Backend Deployment Complete ==="
}

deploy_frontend() {
    log "=== Frontend Deployment ==="

    # 1. Extract
    log "Extracting $FRONTEND_TAR..."
    local tmpdir="$HOME/pm/frontend-new"
    rm -rf "$tmpdir"
    mkdir -p "$tmpdir"
    tar -xzf "$FRONTEND_TAR" -C "$tmpdir"

    # 2. Swap
    log "Swapping old → new..."
    sudo rm -rf "$FRONTEND_DIR-old"
    sudo mv "$FRONTEND_DIR" "$FRONTEND_DIR-old" 2>/dev/null || true
    sudo mkdir -p "$FRONTEND_DIR"
    sudo cp -r "$tmpdir/"* "$FRONTEND_DIR/"
    sudo chown -R www-data:www-data "$FRONTEND_DIR"
    rm -rf "$tmpdir"
    rm -f "$FRONTEND_TAR"

    log "=== Frontend Deployment Complete ==="
}

# ─── Main ────────────────────────────────────────
case "${1:-all}" in
    backend)  deploy_backend ;;
    frontend) deploy_frontend ;;
    all)      deploy_backend; deploy_frontend ;;
    *)
        echo "Usage: bash deploy.sh [backend|frontend|all]"
        exit 1
        ;;
esac
```

```bash
chmod +x ~/deploy.sh

# Allow passwordless sudo for the commands deploy.sh uses
sudo visudo -f /etc/sudoers.d/pm-deploy
```

```
# Add this line (replace abdul with your Linux username):
abdul ALL=(ALL) NOPASSWD: /usr/bin/systemctl, /usr/bin/rm, /usr/bin/mv, /usr/bin/mkdir, /usr/bin/cp, /usr/bin/chown
```

### Step 12.2: Backend CD Workflow (Dual-Mode)

The workflow for `.github/workflows/cd-backend-staging.yml`:

```yaml
name: CD - Backend Staging

on:
  push:
    branches: [staging]
    paths: ['backend/**']
  workflow_dispatch:
    inputs:
      deployment_target:
        description: 'Where to deploy'
        required: true
        type: choice
        default: 'aws'
        options: [aws, self-hosted]

jobs:
  test:
    # Tests always run, regardless of deployment target
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet test backend/

  # ─── AWS PATH (default, unchanged from today) ───
  deploy-aws:
    needs: test
    if: ${{ github.event.inputs.deployment_target == 'aws' || github.event_name == 'push' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
      - name: Login to Amazon ECR
        uses: aws-actions/amazon-ecr-login@v2
      - name: Build and push Docker image
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPO:${{ github.sha }} backend/src/
          docker push $ECR_REGISTRY/$ECR_REPO:${{ github.sha }}
      - name: Terraform apply
        run: |
          cd terraform/environments/staging
          terraform init && terraform apply -auto-approve \
            -var="github_sha=${{ github.sha }}" \
            -var="enable_compute=true"
      - name: Smoke test
        run: curl -f https://api.staging.parcel-management.qawitherev.com/health

  # ─── SELF-HOSTED PATH (manual trigger only) ───
  deploy-self-hosted:
    needs: test
    if: ${{ github.event.inputs.deployment_target == 'self-hosted' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Build
        run: dotnet publish backend/src/ParcelManagement.Api -c Release -o publish/backend

      - name: Package
        run: tar -czf backend.tar.gz publish/backend

      - name: Copy artifact to laptop
        env:
          SSH_KEY: ${{ secrets.LAPTOP_SSH_PRIVATE_KEY }}
        run: |
          mkdir -p ~/.ssh
          echo "$SSH_KEY" > ~/.ssh/deploy_key
          chmod 600 ~/.ssh/deploy_key
          scp -o StrictHostKeyChecking=no -i ~/.ssh/deploy_key \
            backend.tar.gz abdul@ssh.qawitherev.com:/home/abdul/

      - name: Deploy
        run: |
          ssh -o StrictHostKeyChecking=no -i ~/.ssh/deploy_key \
            abdul@ssh.qawitherev.com "bash /home/abdul/deploy.sh backend"

      - name: Verify
        run: curl -f https://api.staging.parcel-management.qawitherev.com/health
```

### How the SSH Connection Works

The GitHub runner just does `ssh abdul@ssh.qawitherev.com`. Here's the path that
connection takes:

```
GitHub Runner                          Laptop
   │                                      │
   │  1. DNS lookup: ssh.qawitherev.com   │
   │     → Cloudflare IPs                 │
   │                                      │
   │  2. SSH to Cloudflare IP:22          │
   │──▶ Cloudflare Edge                   │
   │    "This is for ssh.qawitherev.com"  │
   │    Cloudflare checks: this hostname  │
   │    is routed through tunnel abc123.  │
   │    The laptop has a persistent       │
   │    outbound connection for that      │
   │    tunnel.                           │
   │                                      │
   │    Cloudflare proxies the SSH        │
   │    connection down the tunnel.       │
   │                                      │
   │                                      │  3. cloudflared receives it.
   │                                      │     Ingress rule says:
   │                                      │     ssh.qawitherev.com
   │                                      │     → ssh://localhost:22
   │                                      │
   │                                      │  4. Forwards to SSH server
   │                                      │     on port 22.
   │                                      │
   │◀──────────── SSH session ────────────│
   │     (end-to-end, through tunnel)     │
```

No `cloudflared` on the GitHub runner. No temporary tunnel. No port forwarding.
The laptop's persistent tunnel handles everything. From the runner's perspective,
it's just `ssh abdul@ssh.qawitherev.com`.

Cloudflare Tunnel supports SSH natively — you put `ssh://localhost:22` in the
ingress rules, and Cloudflare proxies TCP port 22 through the tunnel. It doesn't
matter that the runner is on the public internet and the laptop is behind NAT.
The tunnel was initiated from the laptop side, so the connection works.

### The GitHub Secret

You need one secret in your GitHub repo: `LAPTOP_SSH_PRIVATE_KEY`.

Go to repo → Settings → Secrets and variables → Actions → New repository secret.

Name: `LAPTOP_SSH_PRIVATE_KEY`
Value: the content of `~/.ssh/github-actions` from the laptop (the entire file, including the `-----BEGIN OPENSSH PRIVATE KEY-----` and `-----END OPENSSH PRIVATE KEY-----` lines).

### Step 12.3: Frontend CD Workflow (Same Pattern)

```yaml
name: CD - Frontend Staging

on:
  push:
    branches: [staging]
    paths: ['frontend/**']
  workflow_dispatch:
    inputs:
      deployment_target:
        type: choice
        default: 'aws'
        options: [aws, self-hosted]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20.x'
      - run: npm ci
        working-directory: frontend
      - run: npm run build -- --configuration=staging
        working-directory: frontend

  # ─── AWS PATH ───────────────────────────────
  deploy-aws:
    needs: build-and-test
    if: ${{ github.event.inputs.deployment_target == 'aws' || github.event_name == 'push' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20.x'
      - run: npm ci && npm run build -- --configuration=staging
        working-directory: frontend
      - uses: aws-actions/configure-aws-credentials@v4
      - run: aws s3 sync dist/frontend/browser/ s3://parcel-management-staging-frontend/ --delete
      - run: |
          DIST_ID=$(aws cloudfront list-distributions --query "DistributionList.Items[?Aliases.Items[?@=='staging.parcel-management.qawitherev.com']].Id" --output text)
          aws cloudfront create-invalidation --distribution-id $DIST_ID --paths "/*"

  # ─── SELF-HOSTED PATH ───────────────────────
  deploy-self-hosted:
    needs: build-and-test
    if: ${{ github.event.inputs.deployment_target == 'self-hosted' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20.x'

      - name: Build
        run: |
          cd frontend
          npm ci
          npm run build -- --configuration=staging

      - name: Package
        run: tar -czf frontend.tar.gz frontend/dist/frontend/browser/

      - name: Copy to laptop
        env:
          SSH_KEY: ${{ secrets.LAPTOP_SSH_PRIVATE_KEY }}
        run: |
          mkdir -p ~/.ssh
          echo "$SSH_KEY" > ~/.ssh/deploy_key
          chmod 600 ~/.ssh/deploy_key
          scp -o StrictHostKeyChecking=no -i ~/.ssh/deploy_key \
            frontend.tar.gz abdul@ssh.qawitherev.com:/home/abdul/

      - name: Deploy
        run: |
          ssh -o StrictHostKeyChecking=no -i ~/.ssh/deploy_key \
            abdul@ssh.qawitherev.com "bash /home/abdul/deploy.sh frontend"
```

### Runtime Behavior

```
Push to staging branch:
  deploy-aws:           ✅ runs (default)
  deploy-self-hosted:   ❌ skipped

Manual trigger + select "aws":
  deploy-aws:           ✅ runs
  deploy-self-hosted:   ❌ skipped

Manual trigger + select "self-hosted":
  deploy-aws:           ❌ skipped
  deploy-self-hosted:   ✅ runs

Result: One trigger, one build, one deployment target.
        No duplicate work. No confusion.
```

### Production Environment

The same dual-mode pattern applies to production workflows. The only differences:
- Branch: `main` instead of `staging`
- Environment URLs: `parcel-management.qawitherev.com` instead of `staging.parcel-management.qawitherev.com`
- Trigger: `workflow_dispatch` only (manual) for both AWS and self-hosted modes — no automatic push trigger for production

### Troubleshooting Deployments

If the self-hosted deploy fails:

1. **Check GitHub Actions log** for the failed step
2. **Test SSH from anywhere** (your dev machine, not the laptop):
   ```bash
   ssh -i ~/.ssh/github-actions abdul@ssh.qawitherev.com "echo ok"
   ```
   If this works, the tunnel + SSH are fine and the issue is in the deploy script.
3. **Is the persistent tunnel up?**
   ```bash
   # On the laptop
   sudo systemctl status cloudflared
   ```
4. **Is the SSH server running?**
   ```bash
   sudo systemctl status ssh
   ```
5. **Is the SSH key in `authorized_keys`?**
   ```bash
   cat ~/.ssh/authorized_keys
   # Should show the github-actions public key
   ```
6. **Is the DNS record in Cloudflare?**
   Go to Cloudflare dashboard → DNS → look for CNAME `ssh.qawitherev.com`
7. **Is the SSH ingress rule in the tunnel config?**
   ```bash
   grep ssh /etc/cloudflared/config.yml
   # Should show: service: ssh://localhost:22
   ```
8. **Check deploy log on laptop:**
   ```bash
   cat ~/deploy.log
   ```
9. **Check service logs:**
   ```bash
   sudo journalctl -u pm-api -n 50
   ```

---

## 13. Phase 11: Staging Environment

**Goal:** Run staging and production side-by-side on the same laptop.

The approach: staging runs on **different ports**.

| Environment | nginx port | Backend port | Domain |
|---|---|---|---|
| Production | 8080 | 5163 | `parcel-management.qawitherev.com` |
| Staging | 8081 | 5164 | `staging.parcel-management.qawitherev.com` |

### Step 13.1: Create Staging Backend Service

```bash
# Create the staging .env file (different DB, different secrets)
sudo cp /etc/parcel-management/.env /etc/parcel-management/.env.staging
sudo nano /etc/parcel-management/.env.staging
# Change ASPNETCORE_URLS=http://127.0.0.1:5164
# Change ASPNETCORE_ENVIRONMENT=Staging
# Update AllowedOrigins to include staging domain

# Create staging directory
mkdir -p ~/pm/staging-backend

# Build staging backend
cd ~/parcel-management-system/backend/src
dotnet publish ParcelManagement.Api -c Release -o ~/pm/staging-backend
sudo chown -R pm-svc:pm-svc ~/pm/staging-backend
```

Create the staging systemd service:

```bash
sudo nano /etc/systemd/system/pm-api-staging.service
```

```ini
[Unit]
Description=Parcel Management API — Staging
After=network.target

[Service]
Type=simple
User=pm-svc
Group=pm-svc
WorkingDirectory=/home/abdul/pm/staging-backend
EnvironmentFile=/etc/parcel-management/.env.staging
ExecStart=/usr/bin/dotnet /home/abdul/pm/staging-backend/ParcelManagement.Api.dll
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal
SyslogIdentifier=pm-api-staging
NoNewPrivileges=yes
ProtectSystem=strict
ProtectHome=yes
ReadWritePaths=/home/abdul/pm/staging-backend /tmp
PrivateTmp=yes

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now pm-api-staging
```

### Step 13.2: Create Staging nginx Config

```bash
sudo nano /etc/nginx/sites-available/parcel-management-staging
```

This is nearly identical to the production nginx config, but:
- `listen 8081`
- `proxy_pass http://127.0.0.1:5164`
- `root /opt/parcel-management/frontend-staging`

```nginx
limit_req_zone $binary_remote_addr zone=api_staging:10m rate=10r/s;

server {
    listen 8081;
    server_name _;

    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml;

    root /opt/parcel-management/frontend-staging;
    index index.html;

    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;

    location /api/ {
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

    location /health {
        proxy_pass http://127.0.0.1:5164;
    }

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/parcel-management-staging /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx

# Create staging frontend directory
sudo mkdir -p /opt/parcel-management/frontend-staging
echo "<h1>Parcel Management Staging</h1>" | sudo tee /opt/parcel-management/frontend-staging/index.html
```

### Step 13.3: Verify Both Environments

```bash
# Production
curl http://127.0.0.1:8080/health    # → Healthy (port 5163)
curl http://127.0.0.1:8080/           # → Production frontend

# Staging
curl http://127.0.0.1:8081/health    # → Healthy (port 5164)
curl http://127.0.0.1:8081/           # → Staging frontend
```

The Cloudflare Tunnel config already routes both domains (see Phase 6.3 ingress rules):
- `parcel-management.qawitherev.com` → port 8080 (production)
- `staging.parcel-management.qawitherev.com` → port 8081 (staging)

### Step 13.4: Update the Deploy Script for Staging

Add a parameter to the deploy script to handle staging:

```bash
# In ~/deploy.sh, add:
ENVIRONMENT="${2:-production}"

if [ "$ENVIRONMENT" = "staging" ]; then
    BACKEND_OUTPUT="/home/abdul/pm/staging-backend"
    FRONTEND_OUTPUT="/opt/parcel-management/frontend-staging"
    SERVICE_NAME="pm-api-staging"
    NGINX_PORT="8081"
else
    BACKEND_OUTPUT="/home/abdul/pm/backend"
    FRONTEND_OUTPUT="/opt/parcel-management/frontend"
    SERVICE_NAME="pm-api"
    NGINX_PORT="8080"
fi
```

---

## 14. Phase 12: Monitoring & Alerts

**Goal:** Know when your app is down before your users do.

### Step 14.1: Install Uptime Kuma

Uptime Kuma is a self-hosted monitoring tool (like a simple CloudWatch Synthetics).
It runs as a separate systemd service on the same laptop.

```bash
# Install Node.js 20.x (needed for Uptime Kuma)
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Install Uptime Kuma
cd /opt
sudo git clone https://github.com/louislam/uptime-kuma.git
cd uptime-kuma
sudo npm run setup

# Create systemd service
sudo nano /etc/systemd/system/uptime-kuma.service
```

```ini
[Unit]
Description=Uptime Kuma
After=network.target

[Service]
Type=simple
User=pm-svc
WorkingDirectory=/opt/uptime-kuma
ExecStart=/usr/bin/node server/server.js
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now uptime-kuma
```

Access Uptime Kuma at `http://localhost:3001`. Set up monitors:

| Monitor Name | URL | Type | Interval |
|---|---|---|---|
| Production API Health | `http://127.0.0.1:5163/health` | HTTP(s) | 60s |
| Production Frontend | `http://127.0.0.1:8080/` | HTTP(s) | 60s |
| Staging API Health | `http://127.0.0.1:5164/health` | HTTP(s) | 120s |
| Staging Frontend | `http://127.0.0.1:8081/` | HTTP(s) | 120s |
| Cloudflare Tunnel | `http://127.0.0.1:8080/health` | HTTP(s) | 60s |

Configure Discord or Telegram notifications for when monitors go down.

### Step 14.2: Key systemd Commands for Daily Checks

```bash
# Is everything running?
sudo systemctl status pm-api pm-api-staging nginx cloudflared uptime-kuma

# Any errors in the last hour?
sudo journalctl -u pm-api --since "1 hour ago" -p err

# How much disk space is left?
df -h /

# How much RAM is being used?
free -h

# CPU load (should be < 2.0 on a dual-core)
uptime
```

### Step 14.3: Set Up Log Rotation

Logs grow over time. Without rotation, they'll fill your disk.

```bash
sudo nano /etc/systemd/journald.conf
```

```ini
[Journal]
SystemMaxUse=500M
SystemMaxFileSize=100M
MaxRetentionSec=14day
```

```bash
sudo systemctl restart systemd-journald
```

---

## 15. Phase 13: Maintenance & Day-2 Operations

### Daily

```bash
# Quick health check (create an alias for this)
alias pm-status='sudo systemctl status pm-api pm-api-staging nginx cloudflared'
pm-status
```

### Weekly

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Check disk usage
df -h

# Check for failed logins (security)
sudo journalctl -u ssh --since "1 week ago" | grep -i "failed"
```

### Monthly

```bash
# Clean old logs
sudo journalctl --vacuum-time=30d

# Check for .NET runtime updates
dotnet --version
# Compare with latest at https://dotnet.microsoft.com/en-us/download

# Restart the laptop (Windows updates)
# WSL2 restarts automatically on boot
```

### When Things Go Wrong

```bash
# 1. Check what's not running
sudo systemctl list-units --state=failed

# 2. Check the failing service's logs
sudo journalctl -u pm-api -n 100 --no-pager

# 3. Restart the service
sudo systemctl restart pm-api

# 4. If all else fails, restart WSL2
# From PowerShell: wsl --shutdown
# Then reopen Ubuntu from Start menu

# 5. If even that fails, reboot Windows
```

---

## 16. Troubleshooting Guide

### Backend won't start

**Symptom:** `sudo systemctl status pm-api` shows `failed` or `inactive`.

```bash
# Check the full error
sudo journalctl -u pm-api -n 50 --no-pager

# Common causes:
# 1. Missing environment variable → check /etc/parcel-management/.env
# 2. Wrong .dll path → verify ExecStart path in .service file
# 3. Port already in use → lsof -i :5163
# 4. Permissions issue → ls -la ~/pm/backend/ (should be owned by pm-svc)
```

**Symptom:** `Active: active (exited)` — the process started but exited immediately.

This usually means an unhandled exception (database connection failed, missing config).
Check `sudo journalctl -u pm-api -n 50` for the stack trace.

### nginx returns 502 Bad Gateway

**Symptom:** `curl http://127.0.0.1:8080/api/health` returns 502.

This means nginx can't reach the backend. Check:
1. Is the backend running? `sudo systemctl status pm-api`
2. Is the backend listening on the right port? `curl http://127.0.0.1:5163/health`
3. Are the ports correct in the nginx config? `proxy_pass http://127.0.0.1:5163;`

### Cloudflare Tunnel returns 502 or 503

**Symptom:** Browser shows `502 Bad Gateway` from Cloudflare.

1. Is cloudflared running? `sudo systemctl status cloudflared`
2. Is nginx running? `sudo systemctl status nginx`
3. Does localhost routing work? `curl -H "Host: parcel-management.qawitherev.com" http://localhost:8080/health`
4. Check tunnel logs: `sudo journalctl -u cloudflared -n 50`

### DNS not resolving

**Symptom:** Browser shows `DNS_PROBE_FINISHED_NXDOMAIN`.

1. Check Cloudflare DNS dashboard — do the CNAME records exist?
2. Did you run `cloudflared tunnel route dns` for all four hostnames?
3. Did nameserver propagation complete? https://www.whatsmydns.net/
4. Double-check: CNAME target should be `<tunnel-uuid>.cfargotunnel.com`

### WSL2 won't start

**Symptom:** `wsl` command hangs or shows errors.

```powershell
# From PowerShell as admin:
wsl --shutdown
wsl --update
wsl --version

# If still broken:
# Check Windows Features → "Virtual Machine Platform" and "Windows Subsystem for Linux" are enabled
```

### Build too slow on laptop

If `dotnet publish` takes more than 5 minutes:

1. Switch to **pre-built artifacts:** build in GitHub Actions cloud runners, download the `.tar.gz` artifact on the laptop
2. Or: only build what changed (incremental builds with `dotnet build` instead of `dotnet publish`)

### Power outage recovery

When power returns:
1. Laptop should auto-boot (set in BIOS: "Power on when AC is restored")
2. Windows auto-starts the GitHub Actions Runner service
3. WSL2 auto-starts when you open a terminal (or set a scheduled task to run `wsl --exec systemctl is-active pm-api` at boot)
4. systemd auto-starts pm-api, nginx, and cloudflared (because they're all `enabled`)

To make WSL2 auto-start on boot without opening a terminal, create a scheduled task in Windows:

```powershell
# In PowerShell as Administrator
$action = New-ScheduledTaskAction -Execute "wsl.exe" -Argument "--exec sudo systemctl is-active pm-api"
$trigger = New-ScheduledTaskTrigger -AtStartup
Register-ScheduledTask -TaskName "WSL2 Auto Start" -Action $action -Trigger $trigger -RunLevel Highest
```

---

## 17. Full File Reference

Every file you created, in one place for reference:

| File | Purpose | Owner |
|---|---|---|
| `%USERPROFILE%\.wslconfig` | WSL2 memory/CPU limits | Windows |
| `/etc/parcel-management/.env` | Production environment variables | root:root (600) |
| `/etc/parcel-management/.env.staging` | Staging environment variables | root:root (600) |
| `/etc/nginx/sites-available/parcel-management` | Production nginx config | root |
| `/etc/nginx/sites-available/parcel-management-staging` | Staging nginx config | root |
| `/etc/systemd/system/pm-api.service` | Production backend service | root |
| `/etc/systemd/system/pm-api-staging.service` | Staging backend service | root |
| `/etc/systemd/system/uptime-kuma.service` | Monitoring service | root |
| `/etc/cloudflared/config.yml` | Cloudflare Tunnel config | root |
| `~/.cloudflared/<uuid>.json` | Tunnel credentials | user |
| `~/deploy.sh` | Deployment script | user |
| `~/pm/backend/` | Production backend binaries | pm-svc |
| `~/pm/staging-backend/` | Staging backend binaries | pm-svc |
| `/opt/parcel-management/frontend/` | Production frontend static files | www-data |
| `/opt/parcel-management/frontend-staging/` | Staging frontend static files | www-data |

### Quick Start After Reboot

The laptop was rebooted. What needs to happen?

1. **Windows boots** → GitHub Actions Runner service auto-starts
2. **WSL2 boots** (when triggered by scheduled task or first terminal open) →
   - systemd auto-starts: `pm-api`, `pm-api-staging`, `nginx`, `cloudflared`, `uptime-kuma`
3. **cloudflared connects to Cloudflare** → tunnel is established
4. **Everything is online** within ~30 seconds of WSL2 starting

Verify with:
```bash
sudo systemctl status pm-api pm-api-staging nginx cloudflared uptime-kuma
```

---

## Cost Summary

| Item | Before (AWS) | After (Laptop) |
|---|---|---|
| ECS Fargate (2 envs) | ~$14.60/mo | $0 |
| ALB (2 envs) | ~$40.00/mo | $0 |
| NAT Gateways (2 envs) | ~$68.00/mo | $0 |
| S3 + CloudFront | ~$2.00/mo | $0 |
| Route53 Hosted Zones | ~$1.00/mo | $0 |
| Cloudflare | — | Free |
| Electricity (~30W 24/7) | — | ~$2.50/mo |
| **Total** | **~$126/mo** | **~$2.50/mo** |

Annual: ~$1,500 saved. The laptop pays for itself in under 2 months (if you consider
it a sunk cost since you already own it).

---

## Next Steps After Implementation

1. Run in parallel with AWS for 1 week (keep `enable_compute = true` temporarily)
2. Switch DNS to Cloudflare Tunnel
3. Monitor for 48 hours (check Uptime Kuma, Cloudflare Analytics)
4. If stable: set `enable_compute = false` in Terraform, `terraform apply` to destroy AWS compute
5. Keep S3 + CloudFront as cold standby (~$2/mo) or destroy them too
6. Export SSM parameters one final time, then destroy SSM Terraform resources
