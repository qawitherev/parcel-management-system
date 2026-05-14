# User Journey: User-Resident Unit Assignment

## Overview
This feature is intended to allow ParcelRoomManagers and Admins to assign users (residents) to specific residential units. This establishes the relationship: "User X lives in Unit Y" — which is critical for routing parcels to the correct recipients and sending notifications.

**CRITICAL NOTE:** This feature is currently **non-functional** in the frontend. Only a static placeholder page exists.

## Actors
- **ParcelRoomManager** — assigns residents to units
- **Admin** — full access
- **Resident** — no access

---

## Journey (Intended): Assign User to Unit

### Step 1 — Navigate
**Route:** `/resident/userResidentUnit`
**Sidebar path:** Resident > User Resident Unit (visible to Manager, Admin)

### Step 2 — Current reality (GAP)
**What the user actually sees:**

A page with "not this page!" as the heading, followed by a hardcoded static HTML table:

```
| # | Unit Name | Created By | Created At |
|---|-----------|------------|------------|
| 1 | Unit 1    | manager-system | 3pm    |
```

This is **fake data** — hardcoded in the template, not fetched from any API. The component (`AddUserToUnit`) has no:
- Service injection
- API calls
- Form controls
- Reactive data binding
- Any real functionality whatsoever

### Step 3 — Intended journey (what SHOULD happen)

**UI should show:**
1. A list of existing user-unit assignments (which user is assigned to which unit)
2. A form or modal to create a new assignment:
   - Select a **User** (dropdown/search of registered residents)
   - Select a **Resident Unit** (dropdown/search of existing units)
   - Click "Assign"
3. An action to **remove** an assignment (unassign user from unit)

**API endpoints that should exist (based on backend structure):**
- `GET /api/v1/userResidentUnit` — list all assignments (with search + pagination)
- `POST /api/v1/userResidentUnit` — create a new assignment `{ userId, residentUnitId }`
- `DELETE /api/v1/userResidentUnit/{id}` — remove an assignment

**Backend support:**
The backend DOES have the necessary infrastructure:
- `UserResidentUnit` entity exists (Core layer)
- `UserResidentUnitController` exists with endpoints
- `UserResidentUnitService` exists
- `IUserResidentUnitRepository` exists
- `UserResidentUnitSpecification` exists
- `UserResidentUnitDto` exists
- Frontend service file `user-residen-unit-service.ts` exists
- Frontend endpoints file `user-endpoints.ts` exists

The frontend page just was **never implemented** beyond the placeholder.

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **Entire feature is unimplemented** | Critical | The page is a static HTML placeholder. No form, no data binding, no API integration. This is a core relationship — without it, the system can't route parcels to the right residents. |
| 2 | **No user search/selection** | Critical | No way to find and select a user to assign. Needs an autocomplete or searchable dropdown. |
| 3 | **No unit search/selection** | Critical | No way to pick which unit to assign the user to. |
| 4 | **No assignment listing** | Critical | Can't view who is assigned where. The current table is fake static data. |
| 5 | **No unassignment** | High | No way to remove a user from a unit when they move out. |
| 6 | **Registration creates auto-assignment** | Medium | When a resident registers, they provide a `ResidentUnit` field. The backend likely auto-creates the assignment. But there's no UI to fix it if the resident entered the wrong unit. |
| 7 | **No bulk assignment** | Low | For onboarding a new building, assigning 100+ users one by one is tedious. No CSV import for user-unit assignments. |

---

## Backend Endpoint Inventory (for reference)

Based on the backend code structure:
- `UserResidentUnitController` in `Controller/v1/`
- `UserResidentUnitDto` in `DTO/V1/` (response shape)
- `UserResidentUnitService` in `Core/Services/`
- `UserResidentUnit` entity with fields: Id, UserId, ResidentUnitId, CreatedAt, etc.
- Specification pattern and repository are wired up

**The backend is ready — the frontend was never built.**

