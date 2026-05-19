# Workflow naming convention

`ci.yml` — Continuous Integration. Runs on PRs and pushes. Build + test only, no deploy.
`cd-staging.yml` — Continuous Deployment to staging. Runs on push to staging. Build → Terraform → Deploy.
`cd-production.yml` — Continuous Deployment to production. Manual trigger only (workflow_dispatch).
