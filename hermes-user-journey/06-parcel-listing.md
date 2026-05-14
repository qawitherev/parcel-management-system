# User Journey: All Parcels Listing

## Overview
The All Parcels page provides a searchable, filterable, paginated table of every parcel in the system. All roles can access it, giving a comprehensive view of parcel inventory.

## Actors
- **Resident** — sees only their own unit's parcels (assumed server-side scope)
- **ParcelRoomManager** — sees all parcels, manages operations
- **Admin** — sees all parcels

---

## Journey: Browse All Parcels

### Step 1 — Navigate
**Route:** `/parcel/parcels`
**Sidebar path:** Parcel > All Parcels (visible to all roles)
**UI:** 
- "Search Parcels" search bar (left-aligned, with magnifying glass icon via `app-my-searchbar`)
- Status filter dropdown (right-aligned)
- Data table (`app-my-table`) below

### Step 2 — View the table
**Columns:**

| Column | Key | Description |
|--------|-----|-------------|
| Tracking Number | `trackingNumber` | Unique parcel identifier |
| Locker | `locker` | Which locker the parcel is stored in |
| Weight | `weight` | Package weight |
| Dimension | `dimensions` | Package dimensions |
| Resident Unit | `residentUnit` | Which unit the parcel belongs to |
| Status | `status` | Current state: AwaitingPickup or Claimed |

### Step 3 — Search parcels
**Action:** User types in the search bar.
**System behavior:** 
- `searchKeyword` BehaviorSubject emits the keyword
- `combineLatest` triggers a new API call via `parcelService.getAllParcels({ searchKeyword, status, page, take })`
- Table updates reactively (RxJS stream)

### Step 4 — Filter by status
**Action:** User selects a status from the dropdown.
**Options:** "All", "AwaitingPickup", "Claimed"
**System behavior:**
- `statusStream` emits the selected value
- API call re-triggers with new status filter
- Default selection: "All"

**Dropdown styling:** 
- 48 units wide (`w-48`)
- Styled with theme variables for border, text color, focus ring

### Step 5 — Paginate
**Action:** User clicks pagination controls in `app-my-table`.
**System behavior:**
- `PaginationEmitData` emitted with `{ currentPage, pageSize }`
- `paginationParams` BehaviorSubject updates
- API call re-triggers with new page/take params
- Default: page 1, 10 items per page

### Step 6 — Read table data
**Data source:** `parcelList$` observable
**Template:** `@let parcelList = parcelList$ | async` then passes `parcelList.parcels` and `parcelList.count` to the table component.

---

## Reactive Stream Architecture

```
searchKeyword ─────────┐
statusStream ──────────┤── combineLatest ──► map to params ──► switchMap ──► parcelList$
paginationParams ──────┘
```

Key behaviors:
- `distinctUntilChanged()` on status and pagination prevents duplicate API calls
- `switchMap` cancels in-flight requests when a new combination arrives
- No `debounceTime` on search (every keystroke triggers an API call)

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No debounce on search** | Medium | Every keystroke in the search bar fires an API call. For fast typists, this generates many unnecessary requests. Should add `debounceTime(300)`. |
| 2 | **No row click / detail navigation** | High | Rows are not clickable. There's no way to see detailed parcel info or tracking history from the table. Users must copy the tracking number and go to Tracking separately. |
| 3 | **No sorting** | Medium | Table columns have no sort controls. Users can't sort by date, status, unit, etc. The table component (`MyTable`) accepts columns config but has no sort support. |
| 4 | **No date columns** | High | The table doesn't show when a parcel was checked in or claimed. This is critical context — parcels that have been sitting for 2 weeks look identical to parcels checked in 5 minutes ago. |
| 5 | **Limited status filter** | Medium | Only 3 status options. The backend may support more states (e.g. "Overdue", "ReturnToSender") but they're not exposed in the filter. |
| 6 | **No bulk actions from table** | Medium | No checkbox selection, no "Claim Selected" or "Export Selected" actions. Each parcel must be claimed individually via the Claim page. |
| 7 | **No empty state for filtered results** | Low | When a search/filter returns zero results, the table likely shows "No data" generically. No helpful messaging like "No parcels match your search" or "Try a different status filter". |
| 8 | **No export (CSV/Excel)** | Medium | Staff may need to export parcel data for reporting. No export button exists. |
| 9 | **Status displayed as raw enum string** | Low | "AwaitingPickup" is a developer-facing enum name. It should display as "Awaiting Pickup" or "Ready for Collection" with a colored badge/chip. |

