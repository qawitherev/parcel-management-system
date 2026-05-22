# Terraform Infrastructure

## Table of Contents

- [How Modules Work](#how-modules-work)
- [File Roles](#file-roles)
- [Secret Management](#secret-management)
- [Architecture](#architecture)
- [Environment Differences](#environment-differences)
- [Deployment Order](#deployment-order)
- [Common Commands](#common-commands)

---

## How Modules Work

### The Mental Model

A Terraform module is like a **function in code**. You call it with inputs, it creates resources, it returns outputs. The caller doesn't care HOW the resources are created — only what goes in and what comes out.

```
┌─────────────────────────────────────────────────────────────────┐
│  modules/networking/                                            │
│                                                                 │
│  variables.tf        main.tf            outputs.tf              │
│  ────────────        ───────            ──────────              │
│  "What I need"       "What I build"     "What I give back"      │
│                                                                 │
│  vpc_cidr ──────►  aws_vpc.this   ──────► vpc_id               │
│  pub_cidrs ─────►  aws_subnet.pub ──────► public_subnet_ids    │
│  priv_cidrs ────►  aws_subnet.priv ────► private_subnet_ids    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### How Environments Call Modules

Each environment (`staging`, `production`) calls the same modules with different values:

```hcl
# staging/main.tf
module "networking" {
  source    = "../../modules/networking"
  vpc_cidr  = "10.1.0.0/16"     # staging range
}

# production/main.tf
module "networking" {
  source    = "../../modules/networking"
  vpc_cidr  = "10.0.0.0/16"     # production range
}
```

Same module, different inputs → different resources. The blueprint is written once.

### How Modules Chain Together

Modules pass outputs to each other like a pipeline:

```
networking ──► vpc_id ──► security ──► security_group_ids ──► ecs
                │                        │
                │                        ├──► task_role_arns ──► ecs
                │                        │
                └──► public_subnets ──► alb ──► target_group_arn ──► ecs
```

```hcl
# This is how it looks in code:
module "security" {
  vpc_id = module.networking.vpc_id    # ← networking gives vpc_id to security
}

module "alb" {
  public_subnet_ids = module.networking.public_subnet_ids    # ← networking gives subnets to alb
  alb_security_group_id = module.security.alb_security_group_id    # ← security gives SG to alb
}

module "ecs" {
  ecs_service_subnets     = module.networking.private_subnet_ids  # ← networking
  ecs_service_security_groups = [module.security.ecs_security_group_id]  # ← security
  alb_target_group_arn    = module.alb.target_group_arn  # ← alb
}
```

Each `module.X.Y` reference is Terraform saying "wait for X to finish, then pass its output Y to me." This creates the dependency order automatically.

---

## File Roles

### In Each Environment (`environments/{env}/`)

| File | Purpose | Example Content |
|------|---------|----------------|
| `main.tf` | **Blueprint** — what to build | `module "networking" { source = "..." }` |
| `variables.tf` | **Declarations** — what variables exist | `variable "task_cpu" { type = string }` |
| `terraform.tfvars` | **Values** — settings for THIS environment | `task_cpu = "256"` |
| `outputs.tf` | **Results** — values shown after apply | `output "ecs_cluster_name" { ... }` |
| `.terraform.lock.hcl` | **Provider lock** — pins exact provider versions | `hashicorp/aws v5.100.0` |

### In Each Module (`modules/{name}/`)

| File | Purpose |
|------|---------|
| `main.tf` | Resources to create |
| `variables.tf` | Inputs the module accepts |
| `outputs.tf` | Values the module returns |
| `templates/` | Optional — template files (e.g., container definitions) |

### The Difference Between `.tf` and `.tfvars`

```
variables.tf            terraform.tfvars
────────────            ────────────────
DECLARES variables      ASSIGNS values

variable "task_cpu" {   task_cpu = "512"
  type = string
}
```

Both use the same syntax (HCL), but `.tf` files describe WHAT exists, `.tfvars` files provide the VALUES for a specific environment.

---

## Secret Management

Secrets fall into two categories. Each has a different home.

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│  Infrastructure Secrets       App Runtime Secrets   │
│  ─────────────────────        ────────────────────  │
│  AWS access keys              DB connection string  │
│  CI/CD credentials            JWT signing key       │
│                               SendGrid API key      │
│                               Redis password        │
│                                                     │
│       ↓                              ↓              │
│  GitHub Secrets              AWS SSM Parameter Store│
│  (encrypted at rest)         (SecureString, KMS)    │
│                                                     │
│  WHY: Only CI/CD uses them   WHY: ECS injects them  │
│       at deploy time              at container start │
│       as TF_VAR_*                 as env vars        │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Infrastructure Secrets (GitHub Secrets)

Set in GitHub repository settings → Secrets and variables → Actions.

```
AWS_ACCESS_KEY_ID       ← from CI/CD IAM user (Phase 3)
AWS_SECRET_ACCESS_KEY   ← from CI/CD IAM user
```

Used by the pipeline at deploy time:

```yaml
# .github/workflows/tf-deploy.yml
env:
  AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
```

### Runtime Secrets (SSM Parameter Store)

Stored in AWS SSM with SecureString encryption. Follow the naming convention:

```
/{environment}/backend/{ConfigurationKey}
```

Examples:
```
/production/backend/ConnectionStrings__DefaultConnection    (SecureString)
/production/backend/JWTSettings__SecretKey                 (SecureString)
/staging/backend/RedisSettings__ConnectionString            (SecureString)
```

The `__` maps to .NET configuration nesting: `ConnectionStrings__DefaultConnection` → `IConfiguration["ConnectionStrings:DefaultConnection"]`.

ECS injects them at container startup — no files, no plaintext, no GitHub exposure:

```json
{
  "secrets": [
    {
      "name": "CONNECTIONSTRINGS__DEFAULTCONNECTION",
      "valueFrom": "arn:aws:ssm:ap-southeast-1:ACCOUNT:parameter/production/backend/ConnectionStrings__DefaultConnection"
    }
  ]
}
```

### What NOT to Do

```
❌ .env files in the repo        (committed secrets)
❌ terraform.tfvars secrets      (committed plaintext)
❌ Hardcoded values in main.tf   (scattered everywhere)
❌ S3 bucket env files            (plaintext, no audit)
```

---

## Architecture

```
User's Browser
│
├── https://parcel-management.qawitherev.com
│   └── Route53 → CloudFront → S3 (static frontend files)
│       │
│       └── Angular reads config.json → apiUrl = "https://api.parcel-management.qawitherev.com"
│
└── https://api.parcel-management.qawitherev.com
    └── Route53 → ALB (HTTPS, ACM cert) → ECS Fargate (backend :5163)

┌──────────────────────────────────────────────────────────────────┐
│  AWS — ap-southeast-1                                            │
│                                                                  │
│  ┌────────────────────┐    ┌──────────────────────────────────┐ │
│  │ Public Subnets (2) │    │ Private Subnets (2)              │ │
│  │                    │    │                                  │ │
│  │  ┌──────────┐     │    │  ┌──────────┐  ┌──────────────┐ │ │
│  │  │   ALB    │─────┼────┼──►  ECS     │  │    ECR       │ │ │
│  │  │ :80,:443 │     │    │  │ backend  │  │ backend img  │ │ │
│  │  └──────────┘     │    │  │ :5163    │──► repo         │ │ │
│  │                   │    │  └──────────┘  └──────────────┘ │ │
│  └───────────────────┘    │                                  │ │
│                           │  ┌──────────┐                    │ │
│  Internet ←── IGW         │  │ NAT GW   │──► ECR pull       │ │
│                           │  └──────────┘                    │ │
│                           └──────────────────────────────────┘ │
│                                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────────────────┐ │
│  │ Route53  │  │   ACM    │  │ SSM Parameter Store          │ │
│  │ DNS zone │  │ Certs    │  │ /{env}/backend/*              │ │
│  └──────────┘  └──────────┘  └──────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────┘
```

### Module Dependency Graph

```
environments/{env}/main.tf calls modules in this order:

networking ────► security ────► ecr
    │               │              │
    │               │              │
    ▼               ▼              ▼
  dns_certificate ──► cdn ──► dns_records
    │                              │
    ▼                              ▼
   alb ───────────────────────►  ecs
```

### Module Purposes

| Module | Resources Created |
|--------|------------------|
| `networking` | VPC, public/private subnets (2 AZs), IGW, NAT GW, EIP, route tables |
| `security` | ALB security group, ECS security group, task execution IAM role, task IAM role, CI/CD IAM user |
| `ecr` | Backend container image repository with lifecycle policy |
| `dns/certificate` | ACM certificate (ap-southeast-1) for ALB with DNS validation |
| `dns/records` | Route53 A records — api.* → ALB, app.* → CloudFront |
| `cdn` | S3 bucket for frontend, CloudFront distribution, ACM cert (us-east-1) |
| `alb` | Application Load Balancer, target group, HTTP/HTTPS listeners |
| `ecs` | ECS cluster, task definition, service (backend only) |

---

## Environment Differences

Both environments use the same `main.tf` blueprint. All differences come from `terraform.tfvars`:

| Setting | Staging | Production |
|---------|---------|------------|
| VPC CIDR | `10.1.0.0/16` | `10.0.0.0/16` |
| ECS CPU | `256` | `512` |
| ECS Memory | `512` | `1024` |
| Desired Tasks | `1` | `2` |
| Domain | `staging.parcel-management.qawitherev.com` | `parcel-management.qawitherev.com` |
| State File | `s3://.../terraform/staging/terraform.tfstate` | `s3://.../terraform/production/terraform.tfstate` |

### Adding a New Environment

1. Copy `environments/staging/` → `environments/{new-env}/`
2. Change values in `terraform.tfvars`
3. Update the backend `key` in `main.tf`
4. Run `terraform init`

That's it. No module changes. No blueprint duplication.

---

## Deployment Order

### First Time Setup (Manual Steps)

```
1. terraform apply in production
   └── Creates Route53 hosted zone for qawitherev.com

2. Copy Route53 NS values → Namecheap custom DNS
   └── One-time. Now DNS queries go to Route53.

3. terraform apply in staging
   └── Uses data source to read the existing zone.
```

### Normal Deploy (CI/CD)

```
CI Pipeline:
  1. Build & test backend (.NET)
  2. Build & push Docker image to ECR
  3. terraform plan  (in environment directory)
  4. terraform apply (in environment directory)
  5. Wait for ECS service to stabilize

Frontend Pipeline:
  1. Build frontend (npm run build)
  2. aws s3 sync → S3 bucket
  3. aws cloudfront create-invalidation
```

---

## Common Commands

```bash
# Initialize (first time or after module changes)
cd terraform/environments/staging
terraform init

# See what will change
terraform plan

# Apply changes
terraform apply

# See current outputs
terraform output

# Target a single module
terraform plan -target=module.ecs

# Override a variable at runtime (CI/CD)
terraform plan -var="github_sha=${{ github.sha }}"

# Format all .tf files
terraform fmt -recursive

# Validate configuration
terraform validate
```

---

## Cost Saving — `enable_compute` Toggle

The `enable_compute` variable controls whether expensive compute/networking resources are provisioned.
Set it to `false` when the environment is idle to save ~$52/month.

### What it toggles

```
enable_compute = true              enable_compute = false
─────────────────────              ─────────────────────
ALB                    ✓           ALB                    ✗  (~$20/mo saved)
NAT Gateway            ✓           NAT Gateway            ✗  (~$32/mo saved)
EIP (for NAT)          ✓           EIP (for NAT)          ✗
ECS Service            ✓           ECS Service            ✗  (Fargate cost saved)
Route53 API record     ✓           Route53 API record     ✗
CloudWatch dashboard   ✓           CloudWatch dashboard   ✗
─────────────────────────────────────────────────────────
VPC                    ✓           VPC                    ✓  (free)
Subnets                ✓           Subnets                ✓  (free)
Internet Gateway       ✓           Internet Gateway       ✓  (free)
ECR repository         ✓           ECR repository         ✓  (free)
S3 bucket              ✓           S3 bucket              ✓  (free)
CloudFront             ✓           CloudFront             ✓  (free)
Route53 zone           ✓           Route53 zone           ✓  ($0.50/mo)
ACM certificates       ✓           ACM certificates       ✓  (free)
IAM roles/users        ✓           IAM roles/users        ✓  (free)
SSM parameters         ✓           SSM parameters         ✓  (free)
ECS cluster            ✓           ECS cluster            ✓  (free)
ECS task definition    ✓           ECS task definition    ✓  (free)
Security groups        ✓           Security groups        ✓  (free)
Route53 app record     ✓           Route53 app record     ✓  (free)
```

### Usage

```bash
cd terraform/environments/staging   # or production

# Spin down (destroy ALB, NAT, ECS service, API DNS — saves ~$52/mo)
terraform apply -var="enable_compute=false" -var="github_sha=dummy"

# Spin up (rebuild everything — ~2 minutes)
terraform apply -var="enable_compute=true" -var="github_sha=dummy"
```

No file edits needed. No commits. Run directly from your laptop with AWS credentials.

### When to use

| Situation | Action |
|---|---|
| Done working for the day/week | `enable_compute=false` → saves money |
| Ready to develop/deploy again | `enable_compute=true` → full stack back |
| CI/CD deploys | Pipeline uses `terraform.tfvars` default (`true`) |
