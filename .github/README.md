# CI/CD Pipeline Architecture

## Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                           DEVELOPER                                  │
│                                                                      │
│  1. Create feature branch from develop                               │
│  2. Make changes, push, open PR → develop                            │
│  3. Merge PR → develop                                               │
│  4. Merge develop → staging                                          │
│  5. Manual dispatch → production                                     │
└─────────────────────────────────────────────────────────────────────┘

                PR to develop              Push to staging         Manual trigger
                     │                          │                      │
                     ▼                          ▼                      ▼
              ┌─────────────┐          ┌─────────────┐        ┌─────────────┐
              │   ci.yml     │          │cd-staging.yml│        │cd-production│
              │              │          │              │        │   .yml      │
              │ Build + test │          │  Test        │        │             │
              │ No deploy    │          │  Build+push  │        │  Same as    │
              │              │          │  Terraform   │        │  staging    │
              │              │          │  Deploy S3   │        │             │
              │              │          │  Smoke test  │        │             │
              └─────────────┘          └──────┬───────┘        └──────┬──────┘
                                              │                       │
                                              ▼                       ▼
                                     ┌─────────────────────────────────────┐
                                     │            AWS                      │
                                     │                                     │
                                     │  ECR ← Docker image                 │
                                     │  ECS ← Task definition update       │
                                     │  S3  ← Frontend static assets       │
                                     │  CloudFront ← CDN invalidation      │
                                     └─────────────────────────────────────┘
```

## Workflow files

| File | Trigger | Purpose |
|---|---|---|
| `ci.yml` | PR → develop, workflow_dispatch | Build + unit tests + integration tests. No deploy. |
| `cd.yml` | Called by cd-staging/production | Reusable pipeline: test → build → terraform → deploy → smoke. |
| `cd-staging.yml` | Push → staging, workflow_dispatch | Thin caller. Delegates to `cd.yml` with `environment: staging`. |
| `cd-production.yml` | workflow_dispatch only | Thin caller. Delegates to `cd.yml` with `environment: production`. |

## CD Pipeline — detailed flow

```
Push to staging
      │
      ▼
┌──────────────────────────────────────────────────────────────┐
│ Job 1: backend-test                                          │
│                                                               │
│  • dotnet restore (NuGet cache: hashFiles **/*.csproj)       │
│  • dotnet build --warnaserror                                │
│  • dotnet test (unit)                                        │
│  • dotnet test (integration)                                 │
│                                                               │
│  Fails here → pipeline stops. Nothing deployed.              │
└──────────────────────┬───────────────────────────────────────┘
                       │ needs: backend-test
                       ▼
┌──────────────────────────────────────────────────────────────┐
│ Job 2: build-and-push                                        │
│                                                               │
│  • GitHub → OIDC token → AWS STS → temp credentials (1 hour) │
│  • docker build (GHA cache: Docker layers)                    │
│  • docker push to ECR (tag = first 7 chars of git SHA)       │
│                                                               │
│  Output: image_tag = abc1234                                 │
└──────────────────────┬───────────────────────────────────────┘
                       │ needs: build-and-push
                       ▼
┌──────────────────────────────────────────────────────────────┐
│ Job 3: terraform                                              │
│                                                               │
│  • terraform init                                             │
│  • terraform plan -var github_sha=abc1234                    │
│  • terraform apply -auto-approve                              │
│                                                               │
│  What changes:                                                │
│    - ECS task definition v(N+1) → points to image abc1234    │
│    - ECS service → rolling update (new tasks, drain old)     │
│    - Zero downtime                                            │
└──────────────────────┬───────────────────────────────────────┘
                       │ needs: terraform
                       ▼
┌──────────────────────────────────────────────────────────────┐
│ Job 4: deploy-frontend                                        │
│                                                               │
│  • npm ci (cache: package-lock.json hash)                     │
│  • ng build --configuration=staging                          │
│  • aws s3 sync dist/ s3://parcel-management-staging-frontend/│
│  • aws cloudfront create-invalidation --paths "/*"           │
└──────────────────────┬───────────────────────────────────────┘
                       │ needs: deploy-frontend
                       ▼
┌──────────────────────────────────────────────────────────────┐
│ Job 5: smoke-test                                             │
│                                                               │
│  • curl https://api.staging.parcel-management.../health      │
│  • Retry every 10s, up to 30 attempts (5 min)                 │
│  • HTTP 200 → pass. Anything else → pipeline fails.           │
└──────────────────────────────────────────────────────────────┘
```

## Authentication — OIDC (no static keys)

```
GitHub Actions runner
      │
      │ 1. id-token: write → GitHub issues signed JWT
      ▼
    {
      "sub": "repo:qawitherev/parcel-management-system:ref:refs/heads/staging",
      "aud": "sts.amazonaws.com",
      "iss": "https://token.actions.githubusercontent.com"
    }
      │
      │ 2. STS AssumeRoleWithWebIdentity
      ▼
AWS IAM — Trust policy on github-actions-staging
      │
      │  "Allow sts:AssumeRoleWithWebIdentity IF
      │   sub == repo:qawitherev/...:ref:refs/heads/staging"
      │
      │  Match ✓ → Issue temporary credentials (1 hour)
      ▼
    Access key + secret + session token → used by aws CLI, docker, terraform
```

| Role | Trusted branch | Permissions |
|---|---|---|
| `github-actions-staging` | `refs/heads/staging` | AdministratorAccess |
| `github-actions-production` | `refs/heads/main` | AdministratorAccess |

A feature branch cannot assume either role. A push to staging cannot assume the production role. Enforcement is at the IAM level — AWS STS refuses to issue credentials.

## Caching strategy

| Cache | Key | Benefits |
|---|---|---|
| NuGet packages | `hashFiles('**/*.csproj')` | ~200MB skip on cache hit. `restore-keys` fallback for partial hits when csproj changes. |
| npm packages | `hashFiles('package-lock.json')` | Handled by `setup-node`'s built-in `cache: npm`. |
| Docker layers | `type=gha` | Base image + NuGet restore layers cached. Only `COPY . .` and `dotnet publish` rebuild on source changes. |

All caches live on GitHub's blob storage, survive ephemeral runners, and evict after 7 days of inactivity.

## Environments

| | Staging | Production |
|---|---|---|
| Trigger | Push to `staging` | Manual (`workflow_dispatch`) |
| ECS tasks | 1 (256 CPU, 512 MB) | 2 (512 CPU, 1024 MB) |
| VPC CIDR | 10.1.0.0/16 | 10.0.0.0/16 |
| API domain | `api-staging-parcel-management.qawitherev.com` | `api-parcel-management.qawitherev.com` |
| Frontend domain | `staging.parcel-management.qawitherev.com` | `parcel-management.qawitherev.com` |
| GitHub env | `staging` | `production` |

## Example — full deploy cycle

```
1. Developer fixes a bug in AuthService.cs
2. Opens PR: feature/auth-fix → develop
3. ci.yml runs: backend-build → unit-test → integration-test ✓
4. PR approved and merged to develop
5. develop merged to staging
6. cd-staging.yml triggers:

   backend-test (45s)
     → All 42 tests pass ✓
   build-and-push (90s)
     → Docker image tagged "a1b2c3d" pushed to ECR
   terraform (20s)
     → ECS task definition v5 created (image: a1b2c3d)
     → ECS service updates: new tasks launch, pass health checks, old tasks drain
   deploy-frontend (60s)
     → S3 sync, CloudFront invalidate
   smoke-test (10s)
     → curl /health → HTTP 200 ✓

7. Bug is live on staging. QA verifies.
8. Manual dispatch: cd-production.yml (same flow, targets production resources).
```

## Rollback

```
git revert <bad-commit>
git push origin staging
    → New commit → new Docker image tag → new ECS task definition
    → ECS rolls forward to the revert (same as deploying a fix)

Or manually:
    aws ecs update-service --cluster parcel-management-staging \
        --service parcel-management-staging \
        --task-definition parcel-management-staging:<previous-revision>
```

No separate rollback pipeline — the same pipeline deploys the revert commit.
