# User Journey: Locker Management

## Overview
Locker Management allows ParcelRoomManagers and Admins to configure the lockers available in the parcel room. Each locker is a named storage location where parcels are placed during check-in. This is a simple CRUD feature with list, create, and edit capabilities.

## Actors
- **ParcelRoomManager** — manages locker inventory
- **Admin** — full access
- **Resident** — no access (route guarded by `isAdminAndManagerAuthed`)

---

## Journey: View Locker List

### Step 1 — Navigate
**Route:** `/locker`
**Sidebar path:** Locker > Lockers (visible to Manager, Admin)
**UI:**
- Search bar (`app-my-searchbar`) — left-aligned, placeholder "Search lockers"
- "Add Locker" button (`app-my-button`, variant "primary") — right-aligned
- Data table below

### Step 2 — View locker table
**Columns:**

| Column | Key | Description |
|--------|-----|-------------|
| Locker | `lockerName` | Name of the locker |
| Created By | `createdBy` | Who created this locker |
| Created At | `createdAt` | Formatted timestamp |
| Updated By | `updatedBy` | Who last modified |
| Updated At | `updatedAt` | Formatted timestamp |
| Edit | `edit` | Action column with Edit button |

### Step 3 — Search lockers
**Action:** User types in the search bar.
**System behavior:** Same reactive pattern as parcels — `searchKeyword` BehaviorSubject + `combineLatest` triggers API call via `lockerService.getAllLockers()`.

### Step 4 — Paginate
**Action:** User clicks pagination controls.
**System behavior:** `paginationParams` BehaviorSubject updates, default page=1, take=10.

### Step 5 — Click Edit
**Action:** User clicks the Edit action on a locker row.
**System behavior:** `onActionClicked(locker)` calls `onEditClick(locker.id)` → routes to `/locker/addEdit/{id}`.

### Step 6 — Click Add Locker
**Action:** User clicks "Add Locker" button.
**System behavior:** `onAddLockerClick()` → routes to `/locker/addEdit` (no ID).

---

## Journey: Create Locker

### Step 1 — Navigate
**Route:** `/locker/addEdit` (no route param = create mode)
**UI:** Page with heading "Add Locker", a form, and Back/Submit buttons.

### Step 2 — Fill form
| Field | Type | Placeholder | Validation |
|-------|------|-------------|------------|
| Locker Name | text | "Enter locker name" | required, max 20 chars |

### Step 3 — Submit
**Action:** User clicks "Add Locker".
**System behavior:**
- `lockerService.createLocker({ lockerName })` called
- **Success:** Auto-navigates back to `/locker` (via `onBack()`)
- **Failure:** Error shown: `{{ createUpdateRes.message }}` in a red alert box

### Step 4 — Cancel
**Action:** User clicks "Back" button.
**System behavior:** `onBack()` → routes to `/locker`.

---

## Journey: Edit Locker

### Step 1 — Navigate
**Route:** `/locker/addEdit/{id}` (has route param = edit mode)
**System behavior:**
- `AddEdit.ngOnInit()` reads `id` from route params
- Calls `lockerService.getLocker(id)`
- On success, patches `lockerName` into the form

### Step 2 — View current name
**UI:** Form pre-filled with the current locker name. Heading changes to "Edit Locker".

### Step 3 — Modify and save
**Action:** User changes the name and clicks "Update Locker".
**System behavior:**
- `lockerService.updateLocker({ lockerName }, lockerId)` called
- **Success:** Auto-navigates back to `/locker`
- **Failure:** Error displayed

### Error handling for GET:
If `getLocker(id)` fails:
- `lockerDetails.error` is truthy
- Red alert box shown: `{{ lockerDetails.message }}`

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No delete locker** | High | There is no delete action on the locker table. Lockers can only be created and edited. If a locker is no longer in use, it stays in the list forever. |
| 2 | **No confirmation on navigation** | Medium | If the user has unsaved changes in the Add/Edit form and clicks "Back", there's no "Discard changes?" warning. Form state is lost silently. |
| 3 | **No locker capacity/status** | Medium | A locker is just a name. There's no way to know if a locker is full, how many parcels it currently holds, or its maximum capacity. |
| 4 | **Search has no debounce** | Medium | Same issue as parcels list — every keystroke fires an API call. |
| 5 | **Created/Updated timestamps show raw JSON date** | Low | While `formatTime()` is applied in the `map()` operator, if the formatting utility fails or returns unexpected output, the raw timestamp leaks through. |
| 6 | **Edit button on every row** | Low | The Edit action column renders for every row including the header — no visual distinction between actionable rows and header. |
| 7 | **Form buttons use gray color** | Low | Both "Back" and "Add/Update Locker" buttons use the same gray style (`bg-gray-500`). The submit button should be visually distinct (primary color) to guide the user's eye. |
| 8 | **No inline creation** | Low | To add a locker, the user leaves the list page. A simple inline form or modal would be faster for single-field creation. |

