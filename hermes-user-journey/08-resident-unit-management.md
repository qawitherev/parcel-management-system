# User Journey: Resident Unit Management

## Overview
Resident Unit Management allows managers and admins to define the residential units (apartments, suites, rooms) in the building. Each unit is a destination for parcels — when a parcel is checked in, it's assigned to a unit. This is a CRUD feature parallel to Locker Management.

## Actors
- **ParcelRoomManager** — manages building units
- **Admin** — full access
- **Resident** — no access (route guarded)

---

## Journey: View Resident Units List

### Step 1 — Navigate
**Route:** `/residentUnit/units`
**Sidebar path:** Resident Unit > Resident Units (visible to Manager, Admin)
**UI:**
- Search bar (`app-my-searchbar`) — placeholder "Search units..."
- "Add Unit" button — primary variant, right-aligned
- Data table below

### Step 2 — View units table
**Columns:**

| Column | Key | Description |
|--------|-----|-------------|
| Unit Name | `unitName` | Identifier for the unit (e.g., "5B", "Penthouse") |
| Created By | `createdBy` | Who created this unit |
| Created At | `createdAt` | Formatted timestamp |
| Updated By | `updatedBy` | Who last modified |
| Updated At | `updatedAt` | Formatted timestamp |
| Edit | `edit` | Action column with Edit button |

### Step 3 — Search units
**Action:** User types in the search bar.
**System behavior:** `keywordStream` BehaviorSubject triggers reactive API call via `unitsService.getAllUnits()`.

### Step 4 — Paginate
**Action:** User clicks pagination.
**System behavior:** Default page=1, take=10. `paginationParams` BehaviorSubject updates.

### Step 5 — Click Edit
**Action:** User clicks Edit on a row.
**System behavior:** `onEditClick(unit.id)` → routes to `/residentUnit/units/edit/{id}`.

### Step 6 — Click Add Unit
**Action:** User clicks "Add Unit".
**System behavior:** `onAddClick()` → routes to `/residentUnit/units/edit` (no ID = create mode).

---

## Journey: Create Resident Unit

### Step 1 — Navigate
**Route:** `/residentUnit/units/edit` (no route param)
**UI:** Heading "Add Unit", form with one field, Back/Submit buttons.

### Step 2 — Fill form
| Field | Type | Placeholder | Validation |
|-------|------|-------------|------------|
| Unit Name | text | "Enter unit name" | required |

### Step 3 — Submit
**Action:** User clicks "Add Unit".
**System behavior:**
- `unitsService.createUnit({ unitName })` called
- **Success:** Auto-navigates to `/residentUnit/units` via `onBack()`
- **Failure:** Error alert displayed below the input

### Step 4 — Cancel
**Action:** User clicks "Back".
**System behavior:** Routes to `/residentUnit/units`.

---

## Journey: Edit Resident Unit

### Step 1 — Navigate
**Route:** `/residentUnit/units/edit/{id}`
**System behavior:**
- `UnitsEdit.ngOnInit()` reads `id` from route params
- Calls `unitsService.getUnit(id)`
- On success, patches `unitName` into the form. Sets `unitId` so submit knows it's an update.
- **No ID in route:** `switchMap` returns `EMPTY` observable (form stays blank, create mode)

### Step 2 — Modify and save
**Action:** User changes name, clicks "Edit Unit".
**System behavior:**
- `unitsService.updateUnit(unitId, unitName)` called
- **Success:** Navigates back to list
- **Failure:** Error shown

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No delete unit** | High | Cannot remove a unit once created. If a unit is demolished or vacated, it remains in the system permanently. |
| 2 | **No unit occupant/parcel count** | Medium | The table shows only unit metadata. There's no column for "Active Parcels" or "Residents" to indicate unit activity at a glance. |
| 3 | **No import/bulk create** | Low | For buildings with hundreds of units, creating each one manually is impractical. No CSV import like parcels have. |
| 4 | **Same search debounce issue** | Medium | `keywordStream` has no `debounceTime`. Every keystroke triggers an API call. |
| 5 | **Route structure inconsistency** | Low | Edit route is `/residentUnit/units/edit/{id}` (with "edit" in the path) vs. Lockers which uses `/locker/addEdit/{id}` (with "addEdit"). Inconsistent naming convention. |
| 6 | **Back button goes to hardcoded route** | Low | `onBack()` hardcodes `'residentUnit/units'` as string. If the route changes in `app.routes.ts`, this breaks silently. |
| 7 | **No validation for duplicate names** | Medium | Backend likely prevents duplicate unit names, but the error message may not be user-friendly. No client-side uniqueness check. |
| 8 | **No unit detail page** | Low | Clicking a unit row shows the edit form. There's no read-only detail view showing: who lives here, how many parcels are pending, history. |

