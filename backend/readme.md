# 📦 Parcel Management System
A production-quality C# solution that models parcel operations and tracking. Built to showcase clean architecture, maintainability, and pragmatic engineering practices.

• 🖥️ Language: C# (100%)  
• 🗂️ Solution: `parcel-management-system.sln`  
• 📁 Layout: Source under `src/`, project docs under `readme/`

---

## 💡 Why this project
This repository demonstrates how I design and build a real-world backend system with a focus on:

- 🏷️ Clear domain modeling for parcels and delivery workflows
- 🧩 Separation of concerns and testable components
- 🏗️ Maintainable project structure in a multi-project .NET solution
- 👨‍💻 Developer experience: easy restore/build/run with standard .NET tooling

---

## 🚀 Key capabilities
- 📦 Parcel lifecycle management (creation, updates, status transitions)
- 🔄 Assignment and handoff workflows (e.g., to couriers/depots)
- 🔍 Search and listing with common filters (status, recipient, date)
- 📝 Audit-friendly design (status history and timestamps)
- 🧱 Extensible architecture to add integrations (labels, notifications, webhooks)

> ℹ️ Note: The exact feature set depends on the projects under `src/`. The design intentionally supports the above capabilities and can be extended without major refactors.

---

## 🛠️ Tech stack and practices
- 🖥️ C# and .NET (multi-project solution via `parcel-management-system.sln`)
- 🏗️ Dependency Injection and layered/clean architecture
- ✅ Robust validation and error handling patterns
- 📋 Logging-ready design (plug in the provider of your choice)
- ⚙️ Configuration via per-project settings/environment variables
- 🤖 Automated restore/build/test via `dotnet` CLI
- 📝 Scripted SDK setup via `dotnet-install.ps1` (for pinned SDK scenarios)

---

## 📚 Repository structure
```text
/
├─ parcel-management-system.sln     # Solution entry point
├─ src/                              # Application source (projects live here)
├─ readme/                           # Supplemental docs, diagrams, notes
├─ readme.md                         # Repo default readme (legacy)
├─ .gitignore
└─ dotnet-install.ps1                # Optional: install a specific .NET SDK
```

Typical `src/` organization (illustrative):
```text
src/
├─ ParcelManagement.Domain/          # Core entities, value objects, domain logic
├─ ParcelManagement.Application/     # Use cases, services, validation
├─ ParcelManagement.Infrastructure/  # Data access, external integrations
└─ ParcelManagement.Api/             # Web API host (if applicable)
```

---

## 🏁 Getting started

**Prerequisites:**  
- 🟦 .NET SDK installed (use `dotnet --info` to verify)
- 📝 Optionally use `dotnet-install.ps1` to install a specific SDK locally

**Clone and build:**
```bash
git clone https://github.com/qawitherev/parcel-management-system.git
cd parcel-management-system

# Restore and build the whole solution
dotnet restore parcel-management-system.sln
dotnet build   parcel-management-system.sln -c Release
```

**Run** (replace with your startup project if different):
```bash
# Example: run the API or console host project inside src/
dotnet run --project src/ParcelManagement.Api
```

**Tests** (if a test project exists):
```bash
dotnet test parcel-management-system.sln -c Release
```

---

## 🏗️ Architecture overview

- 🏛️ Domain-centric design
  - 📦 Parcel as a primary aggregate with status transitions (e.g., Created → InTransit → Delivered → Returned)
  - 📝 Status history enables auditing and reporting

- 🧠 Application layer
  - 🛠️ Use-case services encapsulate business workflows (create parcel, update status, assign courier)
  - ✅ Validation and DTO mapping at the boundaries

- 🗄️ Infrastructure layer
  - 🗃️ Repository abstractions for persistence
  - 🌐 External services (e.g., notifications, label generation) hidden behind interfaces

- 🌍 API/Host
  - 🧩 Thin HTTP layer exposing use cases (REST endpoints) or a console host for batch workflows
  - 🛡️ Cross-cutting concerns (logging, exception handling) via middleware/pipelines

This separation keeps the domain pure and makes the system easy to test and evolve.

---

## 📝 Example scenarios this system supports

- ✍️ Create a parcel with sender/recipient details and an initial status
- 🔄 Update the parcel’s status as it moves through the delivery pipeline
- 👥 Assign/unassign handlers (couriers, facilities) during transit
- 🔎 Query parcels by status, date range, or recipient for dashboards and operations
- 📊 Produce basic delivery metrics (lead time, failed attempts, returns)

---

## 🏆 Quality and maintainability

- 🧪 Testability: application and domain layers are framework-agnostic
- 🧱 Extensibility: add new statuses, workflows, or integrations with minimal churn
- 👀 Observability-ready: log at boundaries and enrich with correlation IDs
- ⚙️ Configuration-first: isolate environment-specific concerns from code

---

## 🗺️ Roadmap ideas

- 💾 Add persistence (e.g., EF Core or Dapper) with migrations
- ⏳ Introduce background processing for async tasks (e.g., status notifications)
- 📤 Implement an outbound integration (e.g., shipping labels or tracking webhooks)
- 🔐 Add authentication/authorization for operational endpoints
- 📑 Provide OpenAPI/Swagger documentation and Postman collection

---

## 📖 How to read this codebase

1️⃣ Start at `src/` and open the solution `parcel-management-system.sln`  
2️⃣ Review the Domain project to understand entities and invariants  
3️⃣ Explore Application services for workflows and orchestration  
4️⃣ Inspect Infrastructure to see adapters and persistence choices  
5️⃣ Run the host project (API/console) to exercise the use cases end-to-end

---

## 📄 License
If you plan to reuse parts of this code, please check the repository’s license or add one appropriate to your needs.

---

## 🙏 Credits
Built by qawitherev to demonstrate practical C# and solution design skills for a production-style backend.