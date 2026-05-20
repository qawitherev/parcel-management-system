# CI Develop Stage: Lint, Scan & Integration Tests — Explained

> Part of the CI/CD redesign plan
> Target: `ci-develop.yml`

---

## 1. Lint & Scan — What Are They?

### Linting

Linting enforces **code style rules** — indentation, naming conventions, unused imports, etc. It catches bugs that compile but are wrong (e.g., a variable assigned but never read).

### Scanning

Security scanning analyzes code for **vulnerability patterns** — hardcoded secrets, SQL injection, path traversal, known-vulnerable dependencies.

---

## 2. Can We Run These Against Your Codebase?

### .NET Backend: Yes, Immediately

The .NET 9 SDK ships with `dotnet format` built in. Zero setup.

```bash
# Analyze only (report violations, exit code 1 if any found)
dotnet format --verify-no-changes

# Auto-fix violations
dotnet format
```

**What it checks:**
- Unused `using` statements (`IDE0005`)
- Missing/extra whitespace
- Naming convention violations (e.g., private field should be `_camelCase`)
- Code style preferences from `.editorconfig`

**Current state of your codebase:**
Your backend has violations already (I've seen them — `using System.Reflection.Metadata;` and `using Microsoft.AspNetCore.Http.Features;` in `ParcelService.cs` are both unused imports that `dotnet format` would flag). So you'd need to run `dotnet format` once to clean the codebase, then add `dotnet format --verify-no-changes` to CI to prevent regressions.

### Angular Frontend: Needs One Setup Step

Angular 20 includes ESLint support via `ng lint`, but your project doesn't have it configured yet. Run once:

```bash
cd frontend
npx ng add @angular-eslint/schematics
```

This generates `.eslintrc.json` and adds `ng lint` to your scripts. Then in CI:

```bash
npx ng lint --max-warnings=0
```

### CodeQL: Free, One-Click

GitHub CodeQL is free for public repositories. In the workflow:

```yaml
- name: Initialize CodeQL
  uses: github/codeql-action/init@v3
  with:
    languages: csharp, javascript

- name: Build
  run: dotnet build

- name: Perform CodeQL Analysis
  uses: github/codeql-action/analyze@v3
```

CodeQL scans for:
- SQL injection
- Hardcoded credentials
- Path traversal
- XSS
- CSRF
- Deserialization of untrusted data

Zero configuration needed for C# and TypeScript — it auto-detects the codebase.

---

## 3. Integration Tests — GitHub Service Containers

### What They Are

GitHub Actions can run **sidecar containers** alongside your job. Instead of your test code spinning up Docker containers via Testcontainers, GitHub spins up a MySQL container and exposes it on a known port.

### How Your Setup Currently Works

```csharp
// CustomWebApplicationFactory.cs
_mySqlContainer = new MySqlBuilder("mysql:8.0")
    .WithDatabase("integrationTestDB")
    .WithUsername("TestAdmin")
    .WithPassword("AdminPassword123")
    .Build();

await _mySqlContainer.StartAsync();
// Then uses _mySqlContainer.GetConnectionString()
```

Testcontainers dynamically allocates a random port and builds the connection string at runtime. This requires Docker on the CI runner.

### The Problem: GitHub Service Containers vs Testcontainers

GitHub service containers run **before** your test code. They don't integrate with Testcontainers' dynamic port allocation. You'd need to refactor `CustomWebApplicationFactory` to accept a **static connection string** instead of dynamically fetching one from the container.

**This is a non-trivial refactor** — it changes how the entire integration test infrastructure works. The current approach (Testcontainers) is actually **industry standard** and works fine on GitHub's `ubuntu-latest` runners (they have Docker pre-installed).

### Recommendation: Keep Testcontainers, Drop the Service Container Idea

On `ubuntu-latest` GitHub runners, Docker is already installed. Testcontainers works out of the box — no service container needed. The only reason your integration tests failed locally is that we're on macOS without Docker. On CI, it would work.

**No code changes required.** Just run:

```yaml
integration-test:
  runs-on: ubuntu-latest   # ← Docker pre-installed
  needs: unit-test
  steps:
    - checkout
    - setup .NET
    - restore
    - dotnet test src/ParcelManagement.Test.Integration \
        --configuration Release \
        --logger "trx"
```

---

## 4. Revised `ci-develop.yml` Job Structure

With the above analysis, here's the revised lint/scan job and integration test job that actually work with your current codebase:

```yaml
jobs:
  # ── JOB 1: Lint & Scan (fast fail, ~30s-2min) ──
  lint-and-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v5

      # .NET code style
      - uses: actions/setup-dotnet@v5
        with:
          dotnet-version: 9.0.x
      - name: Check .NET formatting
        run: dotnet format --verify-no-changes
        working-directory: backend
        continue-on-error: true   # ← Soft gate initially, harden later

      # CodeQL SAST (free)
      - uses: github/codeql-action/init@v3
        with:
          languages: csharp, javascript
      - name: Build for CodeQL
        run: dotnet build --no-restore
        working-directory: backend
      - uses: github/codeql-action/analyze@v3

  # ── ... build, unit-test jobs ... ──

  # ── JOB 4: Integration Tests (Testcontainers, no refactor) ──
  integration-test:
    runs-on: ubuntu-latest      # ← Docker is pre-installed
    needs: unit-test
    steps:
      - uses: actions/checkout@v5
      - uses: actions/setup-dotnet@v5
        with:
          dotnet-version: 9.0.x
      - name: Run integration tests
        run: dotnet test src/ParcelManagement.Test.Integration \
          --configuration Release \
          --logger "trx;LogFileName=intg-test-result.trx"
        working-directory: backend
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: integration-test-results
          path: backend/**/*.trx
```

---

## 5. What You Need to Do (Checklist)

| Step | Action | Effort |
|------|--------|--------|
| 1 | Run `dotnet format` once to clean backend codebase | 2 min |
| 2 | Add `dotnet format --verify-no-changes` to CI | 1 line |
| 3 | Enable CodeQL in repo Settings → Security → Code scanning | 3 clicks |
| 4 | Add CodeQL workflow step | 5 lines |
| 5 | Run `ng add @angular-eslint/schematics` in frontend | 1 command |
| 6 | Add `ng lint --max-warnings=0` to CI | 1 line |
| 7 | Move integration tests to `ubuntu-latest` (no code changes) | 1 line change |
| 8 | Enable Dependabot (Settings → Security → Dependabot) | 3 clicks |

**Total effort: ~15 minutes of config, zero code refactoring.**

---

## 6. Why the Service Container Idea Was Wrong for This Codebase

The proposal suggested:

```yaml
services:
  mysql:
    image: mysql:8.0
    env:
      MYSQL_ROOT_PASSWORD: test
```

This works for apps that connect to MySQL via a **static connection string** (`server=localhost;port=3306`). But your integration tests use **Testcontainers** — which dynamically allocates ports and passes the connection string programmatically. GitHub service containers allocate static ports, and your code never reads from them. The two approaches are incompatible without rewriting `CustomWebApplicationFactory`.

**The fix:** Don't use service containers. Just use `ubuntu-latest` which has Docker. Testcontainers will spin up its own MySQL container exactly as it does locally. No code changes needed.
