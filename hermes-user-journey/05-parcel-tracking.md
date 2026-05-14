# User Journey: Parcel Tracking

## Overview
Parcel Tracking allows any user (Resident, Manager, Admin) to look up a parcel by its tracking number and view its current status and full event history. The journey spans two pages: a search form and a results page.

## Actors
- **Resident** — tracks their incoming/picked-up parcels
- **ParcelRoomManager** — looks up parcel status for residents who ask in person
- **Admin** — full access

---

## Journey: Search for a Parcel

### Step 1 — Navigate
**Route:** `/parcel/tracking`
**Sidebar path:** Parcel > Tracking (visible to all roles)
**UI:** A centered search interface on a gray background:
- "Search Parcel" heading (large, bold)
- A pill-shaped search bar with a text input and a "Search" button inside a shadowed white container

| Field | Type | Placeholder | Validation |
|-------|------|-------------|------------|
| Search Keyword | text | "Enter tracking number" | required |

### Step 2 — Enter tracking number
**Action:** User types a tracking number.

### Step 3 — Submit search
**Action:** User clicks "Search" button (or presses Enter — form has `ngSubmit`).
**System behavior:**
- The search button is styled blue when the form is valid, grayed out + `cursor-not-allowed` when empty
- No API call is made from this page. Instead, the user is redirected to `/parcel/tracking/searchResult?keyword={trackingNumber}`

**UI state while valid:** Blue button: "bg-blue-600 text-white hover:bg-blue-700"
**UI state while invalid:** Grayed-out: "opacity-50 cursor-not-allowed"

---

## Journey: View Tracking Results

### Step 1 — Arrive at results
**Route:** `/parcel/tracking/searchResult?keyword=ABC123`
**System behavior:**
- `SearchResult.ngOnInit()` reads the `keyword` query param
- Sets `searchKeyword` FormControl to the keyword value
- Calls `trackingService.getUserParcelHistory(keyword)`
- Observable pipes through `catchError(handleApiError)`

### Step 2 — View parcel summary
**UI (success):** A white card (`rounded-xl shadow-lg`) containing:
- **Parcel identifier:** "Parcel {trackingNumber}" as an `h2` heading
- **Current status:** "Status: {currentStatus}" in gray text

### Step 3 — View tracking history
**UI:** Below the summary card, a 4-column grid (`grid-cols-4`) of history items rendered by `app-result-item` components, one per tracking event.

Each `ResultItem` component displays:
- Event timestamp
- Event description (e.g., "Checked In", "Claimed")
- User who performed the action

### Step 4 — Search again from results page
**UI:** A second search bar at the top of the results page with:
- A text input bound to `searchKeyword` FormControl (pre-filled with current query)
- A "Search" button

**Action:** User types a new tracking number and clicks "Search".
**System behavior:** `onSearch()` calls `trackingService.getUserParcelHistory(this.searchKeyword.value)` and updates `parcelsTrackingHistory$` in-place without changing the URL.

### Step 5 — Handle errors
**UI (error):** A red error card (`bg-red-50 border-red-300`) with an SVG error icon and message:
- "Error: {res.message}"

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **Search page is a redundant step** | Low | The `/parcel/tracking` search page just redirects to `/parcel/tracking/searchResult` with a query param. The same search bar also exists on the results page. Two pages where one would suffice adds unnecessary navigation. |
| 2 | **No URL update on re-search** | Medium | When searching again from the results page, the URL stays at the old `?keyword=`. Browser back button won't return to the previous search. |
| 3 | **Search from results doesn't update the query param** | Medium | `onSearch()` updates only the observable, not the route. If the user refreshes, they lose their current search and revert to the initial query param. |
| 4 | **No "no results" state** | Medium | If a tracking number returns no results, the behavior is unclear. The template uses `*ngIf="parcelsTrackingHistory$ | async as res"` — if the API returns a 404 or empty response, what renders? No empty state is defined. |
| 5 | **History items lack visual timeline** | High | History events are rendered in a flat 4-column grid with no connecting line, no chronological ordering indication. A vertical timeline with dots and lines would be much more intuitive for tracking event sequences. |
| 6 | **Tracking shows ALL parcels, not just user's** | Medium | The endpoint is `getUserParcelHistory` but the name is misleading — based on the API layer, tracking might show any parcel by tracking number regardless of user association. Privacy concern if not scoped. |
| 7 | **No real-time/live status indicator** | Low | Status is fetched once on page load. No polling, no WebSocket updates. If a parcel gets claimed while viewing, the user won't know without refreshing. |
| 8 | **No parcel detail beyond status** | Medium | The result shows tracking number + status + history but omits: locker location, weight, dimensions, resident unit. Users must go to `/parcel/parcels` to see those details. |

