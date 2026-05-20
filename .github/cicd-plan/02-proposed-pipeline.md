# Proposed CI/CD Pipeline: Best Practices Design

> **Repo:** parcel-management-system
> **Date:** 2026-05-14
> **Branch:** agentic-hermes/cicd-plan

---

## Philosophy

- **Shift left:** catch issues at the earliest possible stage
- **Hermetic builds:** every CI run starts from a clean ephemeral environment
- **Immutable artifacts:** build once, promote the same artifact through environments
- **Defense in depth:** multiple quality gates before production
- **Fail fast:** stop the pipeline immediately on first failure

---

## Pipeline Stages Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        DEVELOP BRANCH                        │
│                                                             │
│  PR opened ──► CI (build + lint + test + scan) ──► merge   │
│  Push       ──► CI (build + lint + test + scan)             │
└──────────────────────────┬──────────────────────────────────┘
                           │ merge PR
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                       STAGING BRANCH                         │
│                                                             │
│  Push ──► CI (build + lint + test + scan)                   │
│       ──► Build Docker image ──► Push to registry           │
│       ──► Deploy to staging ──► Smoke test                  │
│       ──► Integration test against staging                  │
└──────────────────────────┬──────────────────────────────────┘
                           │ promote (manual approval)
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                         MAIN BRANCH                           │
│                                                             │
│  PR from staging ──► Production dry-run plan                │
│  Merge ──► Retag staging image as production                │
│       ──► Terraform plan (review required)                  │
│       ──► Terraform apply (manual approval gate)            │
│       ──► Smoke test ──► Health check                       │
│       ──► Rollback on failure                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Workflow Files (7 total, all new)

| File | Purpose |
|------|---------|
| `ci-develop.yml` | PR + push CI for develop branch |
| `ci-staging.yml` | Push CI for staging branch |
| `cd-staging.yml` | Deploy to staging environment |
| `cd-production-plan.yml` | Terraform plan for production |
| `cd-production-apply.yml` | Terraform apply + smoke test |
| `cd-rollback.yml` | Manual rollback trigger |
| `_shared-actions.yml` | Reusable composite actions |

---

## Stage 1: `ci-develop.yml` — Developer CI Gate

**Triggers:**
- `pull_request` targeting `develop`
- `push` to `develop`

**Why:** PRs must pass before merge (branch protection requirement). Push trigger catches direct pushes or merge commits that bypass PR checks.

```yaml
name: CI - Develop

on:
  pull_request:
    branches: [develop]
  push:
    branches: [develop]

jobs:
  # ── JOB 1: Static Analysis (fast, runs first) ──
  lint-and-scan:
    runs-on: ubuntu-latest          # ← Ephemeral, not self-hosted
    steps:
      - checkout
      - setup .NET 9.0.x
      - dotnet format --verify-no-changes    # Code style enforcement
      - ESLint for frontend                  # Frontend linting
      - CodeQL analysis                      # SAST (free for public repos)
      - Dependency review (Dependabot)       # Vulnerability scanning

  # ── JOB 2: Build ──
  build:
    runs-on: ubuntu-latest
    needs: lint-and-scan            # ← Only build if lint passes
    steps:
      - checkout
      - setup .NET 9.0.x
      - restore + build (Release, --warnaserror)

  # ── JOB 3: Unit Tests ──
  unit-test:
    runs-on: ubuntu-latest
    needs: build
    steps:
      - checkout
      - dotnet test (ParcelManagement.Test)
      - npm test (frontend)
      - upload TRX + JUnit reports

  # ── JOB 4: Integration Tests ──
  integration-test:
    runs-on: ubuntu-latest
    needs: unit-test
    services:                       # ← Use GitHub service containers
      mysql:
        image: mysql:8.0
        env: ...
    steps:
      - checkout
      - dotnet test (ParcelManagement.Test.Integration)

  # ── JOB 5: Build Docker (dry-run, no push) ──
  docker-dry-run:
    runs-on: ubuntu-latest
    needs: integration-test
    steps:
      - docker build backend (push: false)
      - docker build frontend (push: false)
```

**Why each step:**

| Step | Reason |
|------|--------|
| `ubuntu-latest` | Hermetic — fresh VM every run. No state pollution from previous builds. |
| Lint first | Fast failure (~30s). Don't waste minutes building code that fails style checks. |
| CodeQL | Free SAST for public repos. Catches SQL injection, hardcoded secrets, path traversal. |
| Dependabot | Native GitHub integration. Auto-PRs for vulnerable dependencies. |
| `needs:` chains | Sequential dependency reduces wasted runner minutes. If lint fails, nothing else runs. |
| Service containers | Replaces Testcontainers Docker dependency. MySQL spins up as a GitHub-managed sidecar. |
| Docker dry-run | Validates Dockerfile without pushing. Catches Docker build errors in CI, not in CD. |

---

## Stage 2: `cd-staging.yml` — Staging Deployment

**Triggers:**
- `push` to `staging`

**Why:** Every push to staging gets a full CI run, then deploys if all gates pass. This is the "integration" environment — the first place all services run together.

```yaml
name: CD - Staging

on:
  push:
    branches: [staging]

jobs:
  # ── JOB 1: Full CI (same as develop, reusing composite actions) ──
  ci:
    uses: ./.github/workflows/ci-develop.yml  # ← Reuse, don't duplicate

  # ── JOB 2: Build + Push Docker Images ──
  build-and-push:
    needs: ci
    runs-on: ubuntu-latest
    steps:
      - docker build backend → tag: ghcr.io/owner/backend:staging-{sha}
      - docker build frontend → tag: ghcr.io/owner/frontend:staging-{sha}
      - push to GHCR (not DockerHub for staging)

  # ── JOB 3: Deploy to Staging Server ──
  deploy:
    needs: build-and-push
    runs-on: self-hosted              # ← Only here: needs Docker socket
    environment: staging
    steps:
      - pull images from GHCR
      - docker compose up -d
      - retry health check (10 attempts, 2s apart)
      - on failure: docker compose down, fail pipeline

  # ── JOB 4: Smoke Tests ──
  smoke-test:
    needs: deploy
    runs-on: ubuntu-latest
    steps:
      - curl POST /api/v1/User/login (verify 200)
      - curl GET /health (verify healthy)
      - curl GET / (verify frontend serves)
```

**Why each step:**

| Step | Reason |
|------|--------|
| Reuse `ci-develop.yml` | DRY. Staging CI is identical to develop CI. One source of truth. |
| GHCR instead of DockerHub | GitHub Container Registry is free for public repos, integrated with GitHub auth, and keeps artifacts close to the code. |
| Image tag includes SHA | Immutable artifact. You know exactly which commit is deployed. |
| Self-hosted only for deploy | Self-hosted runner has Docker socket for `docker compose`. CI stays ephemeral. |
| Smoke tests after deploy | Validates the deployment actually works. Catches config/secrets issues that build can't. |
| Shutdown on failure | Prevents broken staging from running indefinitely. Clean failure state. |

---

## Stage 3: `cd-production-plan.yml` — Production Dry Run

**Triggers:**
- `pull_request` targeting `main`

**Why:** Before merging to main, show exactly what Terraform will change. This is a review gate — the plan is attached to the PR.

```yaml
name: CD - Production Plan

on:
  pull_request:
    branches: [main]

jobs:
  terraform-plan:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - checkout
      - configure AWS credentials (OIDC, not long-lived keys)
      - terraform init
      - terraform plan -out=tfplan
      - upload tfplan as PR comment (via github-script)

  # Optional: cost estimation
  infracost:
    runs-on: ubuntu-latest
    steps:
      - infracost breakdown (posts cost diff as PR comment)
```

**Why each step:**

| Step | Reason |
|------|--------|
| PR trigger on main | Reviewers see the Terraform diff before approving. No surprises on merge. |
| OIDC for AWS | No long-lived AWS credentials stored as secrets. Short-lived tokens via GitHub → AWS federation. |
| Plan as PR comment | Transparency. Reviewer doesn't need to open Actions to see what will change. |
| Infracost (optional) | Shows cost impact of infrastructure changes before they're applied. |

---

## Stage 4: `cd-production-apply.yml` — Production Deployment

**Triggers:**
- `push` to `main`
- `workflow_dispatch` (manual with image tag input)

**Why:** Merge to main = deploy to production. Manual trigger for rollbacks or hotfixes.

```yaml
name: CD - Production Apply

on:
  push:
    branches: [main]
  workflow_dispatch:
    inputs:
      image-tag:
        description: 'Docker image tag to deploy'
        required: true

jobs:
  # ── JOB 1: Promote Image ──
  promote-image:
    runs-on: ubuntu-latest
    steps:
      - pull staging-{sha} from GHCR
      - retag as latest + prod-{sha}
      - push to DockerHub (or ECR)

  # ── JOB 2: Terraform Apply ──
  terraform-apply:
    needs: promote-image
    runs-on: ubuntu-latest
    environment: production           # ← Requires manual approval in GitHub
    steps:
      - checkout
      - configure AWS (OIDC)
      - terraform init
      - terraform apply -auto-approve (with new image tag)
      - wait for ECS services-stable

  # ── JOB 3: Post-Deploy Smoke Test ──
  smoke-test:
    needs: terraform-apply
    runs-on: ubuntu-latest
    steps:
      - health check production endpoint
      - curl critical API endpoints
      - on failure: trigger rollback workflow
```

**Why each step:**

| Step | Reason |
|------|--------|
| Promote, don't rebuild | The image deployed to prod is the exact same binary tested in staging. No rebuild = no risk of different dependencies. |
| `environment: production` | Enables GitHub's **manual approval gate**. A human must click "approve" before Terraform runs. |
| Manual `workflow_dispatch` | Enables rollback: deploy a previous image tag without touching code. |
| Post-deploy smoke test | Catches deployment issues within seconds. If the smoke test fails, the rollback workflow fires automatically. |
| `aws ecs wait services-stable` | Blocks until ECS reports the new task definition is healthy. Prevents the pipeline from reporting success before the deployment actually stabilizes. |

---

## Stage 5: `cd-rollback.yml` — Emergency Rollback

**Triggers:**
- `workflow_dispatch` (manual)
- Called automatically by smoke test failure in production

```yaml
name: CD - Rollback

on:
  workflow_dispatch:
    inputs:
      target-environment:
        type: choice
        options: [staging, production]

jobs:
  rollback:
    runs-on: ubuntu-latest
    environment: ${{ inputs.target-environment }}
    steps:
      - determine previous image tag (from deployment history)
      - if ECS: update task definition to previous revision
      - if docker compose: docker compose up with previous tag
      - health check
      - notify Slack/Discord
```

**Why:**
- MTTR (Mean Time to Recovery) is the #1 SRE metric. Automated rollback reduces it from minutes to seconds.
- Slack notification ensures the team knows about the rollback immediately.

---

## Shared Infrastructure

### Composite Actions (`.github/actions/`)

Replace the monolithic `spin-dotnet` with focused, composable actions:

| Action | Does |
|--------|------|
| `setup-dotnet/action.yml` | Setup .NET SDK only (no checkout) |
| `setup-node/action.yml` | Setup Node.js only |
| `restore-cache/action.yml` | NuGet + npm restore with caching |
| `health-check/action.yml` | Retry loop with curl, configurable endpoint + attempts |

### Branch Protection Rules (GitHub Settings)

```
develop:
  - Require pull request before merging: ✅
  - Require status checks: ci-develop / lint-and-scan, build, unit-test
  - Require branches to be up to date: ✅
  - Do not allow bypassing: ✅

staging:
  - Require pull request before merging: ✅
  - Require status checks: ci-staging / lint-and-scan, build, unit-test, integration-test
  - Do not allow bypassing: ✅

main:
  - Require pull request before merging: ✅
  - Require status checks: cd-production-plan / terraform-plan
  - Require approvals: 1
  - Do not allow bypassing: ✅
```

---

## Migration Path (What to Delete)

| Current File | Action |
|-------------|--------|
| `ci-pr-develop-backend.yml` | Replace with `ci-develop.yml` |
| `ci-push-backend.yml` | Absorbed into `ci-develop.yml` (push trigger) |
| `ci-push-frontend.yml` | Absorbed into `ci-develop.yml` (adds frontend test job) |
| `cd-push-stag-backend.yml` | Replace with `cd-staging.yml` |
| `cd-push-stag-frontend.yml` | Replace with `cd-staging.yml` (unified) |
| `cd-push-prod-backend-frontend.yml` | Split into `cd-production-plan.yml` + `cd-production-apply.yml` |
| `cd-push-prod-frontend.yml` | **Delete** (already deprecated) |
| `convention.md` | Archive — naming convention changes |
| `spin-dotnet/action.yml` | Replace with focused single-purpose actions |

---

## Quick Wins (Do Immediately)

These require zero new workflows — just config changes or minor edits:

1. **Change CI `runs-on` from `self-hosted` to `ubuntu-latest`** in `ci-pr-develop-backend.yml`
2. **Delete `cd-push-prod-frontend.yml`** — dead code
3. **Fix TF apply condition** — change to `exitcode == 2` only
4. **Replace `sleep 20`** with retry loop in staging backend CD
5. **Enable branch protection enforcement** on `develop` and `staging`
6. **Remove checkout from `spin-dotnet`** composite action
7. **Add Dependabot** — create `.github/dependabot.yml` for NuGet + npm
