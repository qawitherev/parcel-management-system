# CI/CD Audit: Current Pipeline Review

> **Repo:** parcel-management-system
> **Date:** 2026-05-14
> **Branch:** agentic-hermes/cicd-plan

---

## Current Workflow Inventory

| File | Triggers | What It Does |
|------|----------|-------------|
| `ci-pr-develop-backend.yml` | PR → develop | Build backend, unit tests, integration tests |
| `ci-push-backend.yml` | Manual only | Build + unit tests (no integration tests) |
| `ci-push-frontend.yml` | Manual only | npm ci + frontend unit tests |
| `cd-push-stag-backend.yml` | Push → staging (backend paths) | Docker build + docker compose up + health check |
| `cd-push-stag-frontend.yml` | Push → staging (frontend paths) | Docker build + docker compose up + health checks |
| `cd-push-prod-backend-frontend.yml` | Manual only | Docker build/push to DockerHub + Terraform plan/apply (ECS) |
| `cd-push-prod-frontend.yml` | Manual only | **DEPRECATED** — dead code |

---

## Mistake #1: Self-hosted runners for CI

**What's wrong:**
`ci-pr-develop-backend.yml` runs on `runs-on: self-hosted`. Self-hosted runners maintain state between runs — leftover artifacts, cached NuGet packages, Docker images, zombie processes. This makes builds non-reproducible. A passing build on the self-hosted runner today could fail tomorrow because of accumulated cruft.

**Why it matters:**
CI must be **hermetic** — each run starts from a clean slate. GitHub-hosted runners (`ubuntu-latest`) are ephemeral VMs destroyed after each job. This guarantees reproducibility.

**Fix:** Move all CI jobs (build, test, lint) to `ubuntu-latest`. Reserve self-hosted runners for CD jobs that need Docker socket access.

---

## Mistake #2: No test gate on staging deployment

**What's wrong:**
`cd-push-stag-backend.yml` and `cd-push-stag-frontend.yml` trigger on push to `staging` and immediately build Docker images and run `docker compose up`. There is **zero testing** between push and deploy. If a broken commit reaches staging, it gets deployed with no verification.

**Why it matters:**
Staging should mirror production behavior. If you deploy untested code to staging, you've defeated the purpose of having a staging environment. The pipeline should run the full test suite on staging before deployment.

**Fix:** Either (a) gate staging CD on a successful CI run on the source branch, or (b) add a test job inside the CD workflow that runs before the Docker build.

---

## Mistake #3: No linting, static analysis, or security scanning

**What's wrong:**
The only code quality gate is `--warnaserror` during `dotnet build`. There is no:
- .NET code analysis (CA rules)
- ESLint for Angular frontend
- Secret scanning (GitHub secret scanning, truffleHog, gitleaks)
- Dependency vulnerability scanning (Dependabot, Snyk, OWASP)
- SAST (SonarQube, CodeQL)
- Container image scanning (Trivy, Docker Scout)

**Why it matters:**
Code that compiles is not necessarily safe or maintainable. A secret committed 3 weeks ago still exists in git history. A vulnerable NuGet package won't be detected until exploited.

**Fix:** Add at minimum: CodeQL (free for public repos), Dependabot (built into GitHub), and `dotnet format --verify-no-changes`.

---

## Mistake #4: Naive `sleep 20` instead of proper readiness probes

**What's wrong:**
`cd-push-stag-backend.yml` line 56:
```yaml
- name: wait for all services wake up
  run: sleep 20
```

This is a race condition. On a slow machine, 20 seconds may not be enough. On a fast machine, it wastes time. The frontend staging workflow (lines 53-90) has proper retry loops with `curl` — the backend workflow should too.

**Why it matters:**
Flaky deployments. The health check on line 63 runs immediately after `sleep 20` with no retries. If the service takes 21 seconds, the deployment falsely fails.

**Fix:** Replace with a retry loop identical to the frontend workflow (max 10 attempts, 2s sleep between).

---

## Mistake #5: Inconsistent backend/frontend CD quality

**What's wrong:**
The staging CD workflows for backend and frontend have different logic quality:

| Feature | Backend | Frontend |
|---------|---------|----------|
| Health check retries | None (single shot) | 10 attempts with 2s delay |
| Wait strategy | `sleep 20` | Retry loop |
| Shutdown on failure | Conditional on health check | Conditional on either check |

These should be identical. The backend workflow is lower quality.

**Why it matters:**
Inconsistency breeds bugs. A pattern that works for frontend but is missing from backend means backend deployments are less reliable.

**Fix:** Unify into a single staging deployment workflow with a shared health-check composite action.

---

## Mistake #6: Staging images not pushed to a registry

**What's wrong:**
Both staging CD workflows set `push: false` for Docker images. The images exist only on the self-hosted runner's local Docker daemon. If the runner is recycled, images are lost. There's no way to pull the same image for debugging or promotion.

**Why it matters:**
The image you test in staging should be the **exact same image** you promote to production. Without a registry, you rebuild from source for production — which may produce a different binary (different timestamps, different dependency resolution).

**Fix:** Push staging images to a registry (DockerHub, GHCR, ECR) with a `staging-{sha}` tag. Production promotion should retag, not rebuild.

---

## Mistake #7: Production Terraform apply on exit code 0

**What's wrong:**
`cd-push-prod-backend-frontend.yml` line 113:
```yaml
if: needs.setup-terraform-and-build-plan.outputs.tfPlanExitCode == 2 || needs.setup-terraform-and-build-plan.outputs.tfPlanExitCode == 0
```

Terraform `-detailed-exitcode` returns:
- `0` = no changes
- `1` = error
- `2` = changes exist

The condition allows apply when exit code is `0` (no changes). This wastes time and adds noise. The comment on line 112 even says `"TODO: to change this into exit code == 2 only"`.

**Why it matters:**
Every production deployment triggers a Terraform apply, even when nothing changed. This is unnecessary API calls against AWS, unnecessary state file writes, and unnecessary risk.

**Fix:** Change to `if: ... == 2` only.

---

## Mistake #8: Dead code in the repo

**What's wrong:**
`cd-push-prod-frontend.yml` has a `# DEPRECATED` comment but still lives in `.github/workflows/`. GitHub will still show it as a valid workflow. The production deployment was consolidated into `cd-push-prod-backend-frontend.yml` but the old file was never deleted.

**Why it matters:**
Dead code is a maintenance burden and a security risk. Someone might accidentally re-enable it. New team members get confused about which workflow is authoritative.

**Fix:** Delete the file. It's in git history if needed.

---

## Mistake #9: Composite action does too much

**What's wrong:**
`.github/actions/spin-dotnet/action.yml` includes `actions/checkout@v5` as its first step. Composite actions should be **single responsibility**. Checkout is a workflow-level concern, not an action concern. If someone wants to use `spin-dotnet` with a different checkout depth or ref, they can't.

**Why it matters:**
Violates the principle of composability. The action name says "spin dotnet" but it also checks out code. Unrelated concerns coupled together.

**Fix:** Remove the checkout step from the action. Callers should checkout first, then use the action.

---

## Mistake #10: No CI on push to main branches

**What's wrong:**
There is no workflow triggered by `push` to `develop` or `staging`. Only PR into develop has CI. If someone force-pushes to develop (bypassing branch protection), no tests run. If a hotfix is committed directly, no tests run.

**Why it matters:**
Branch protection rules can be bypassed (as evidenced by our direct pushes to develop). The CI system should be the last line of defense — run tests on every push to protected branches.

**Fix:** Add a `push` trigger for `develop` and `staging` branches that runs the full test suite.

---

## Mistake #11: Integration tests are CI-only, not in push workflows

**What's wrong:**
`ci-push-backend.yml` runs only unit tests (line 67: `ParcelManagement.Test`). Integration tests (`ParcelManagement.Test.Integration`) are only run in the PR workflow. If integration tests break due to a merge, you won't know until the next PR.

**Why it matters:**
Integration tests catch real-world failures (database interactions, HTTP pipeline, auth). If they're only run on PRs, a broken merge tomain can go undetected for days.

**Fix:** Run integration tests in all CI workflows, not just PRs.

---

## Mistake #12: No deployment rollback

**What's wrong:**
None of the CD workflows have a rollback mechanism. If a bad image reaches production via Terraform, there's no automatic way to revert. The operator must manually run Terraform with the previous SHA.

**Why it matters:**
Mean Time To Recovery (MTTR) is the most important SRE metric. Without automated rollback, every bad deploy is a manual firefight.

**Fix (future):** Store the previous task definition ARN in a Terraform output or SSM parameter. Add a `workflow_dispatch` rollback workflow that redeploys the previous image.

---

## Mistake #13: No branch protection enforcement

**What's wrong:**
We've been pushing directly to `develop` with `--force` (bypassing the "Changes must be made through a pull request" rule). GitHub's branch protection warned us but didn't stop us. This means the rule is set to "warn" not "enforce".

**Why it matters:**
Without enforcement, `develop` can receive unreviewed, untested code. The PR-based workflow is the backbone of code review and quality gates.

**Fix:** Enable branch protection enforcement on `develop`, `staging`, and `main`. Require CI to pass before merge.

---

## Summary: Severity Matrix

| # | Issue | Severity | Effort to Fix |
|---|-------|----------|---------------|
| 1 | Self-hosted CI runners | Medium | Low (change `runs-on`) |
| 2 | No test gate on staging deploy | High | Medium |
| 3 | No linting/security scanning | High | Medium |
| 4 | `sleep 20` instead of readiness probe | Medium | Low |
| 5 | Inconsistent CD quality | Low | Medium |
| 6 | Staging images not in registry | Medium | Medium |
| 7 | TF apply on no-changes | Low | Low (one line) |
| 8 | Dead workflow file | Low | Low |
| 9 | Composite action scope creep | Low | Low |
| 10 | No CI on push to main branches | High | Low |
| 11 | Integration tests skipped in push | Medium | Low |
| 12 | No deployment rollback | Medium | High |
| 13 | Branch protection not enforced | High | Low (GitHub settings) |
