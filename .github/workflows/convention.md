# Workflow naming convention

`ci.yml` — Continuous Integration. Runs on PRs to develop. Build + test only, no deploy.

`cd.yml` — Shared CD logic (reusable workflow). Not triggered directly. Called by `cd-staging.yml` and `cd-production.yml`.

`cd-staging.yml` — Continuous Deployment to staging. Triggered by push to staging. Thin caller → delegates to `cd.yml`.

`cd-production.yml` — Continuous Deployment to production. Manual trigger only (`workflow_dispatch`). Thin caller → delegates to `cd.yml`.
