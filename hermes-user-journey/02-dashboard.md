# User Journey: Dashboard

## Overview
The Dashboard serves as the landing page after login. It presents different views based on user role: **Admin/Manager** see system-wide parcel statistics, while **Residents** see their personal parcel status.

## Actors
- **Resident** — sees personal awaiting-pickup count + welcome message
- **ParcelRoomManager / Admin** — sees system-wide awaiting-pickup and recently-picked-up counts

---

## Journey: Admin / Manager Dashboard

### Step 1 — Arrive after login
**Route:** `/dashboard/user` (or `/dashboard` with redirect)
**Role check:** `Resident`, `ParcelRoomManager`, `Admin`
**UI:** A two-column grid with two stat cards.

### Step 2 — View "Parcels Awaiting Pickup"
**Component:** `AwaitingPickup`
**API:** `GET /api/v1/dashboard/awaiting-pickup` (assumed)
**UI:** White card with:
- Title: "Parcels Awaiting Pickup"
- Large blue number showing the count
- `text-3xl font-bold text-blue-600`

### Step 3 — View "Recently Picked Up"
**Component:** `RecentlyPickedUp`
**API:** `GET /api/v1/dashboard/recently-picked-up` (assumed)
**UI:** White card with:
- Title: "Recently Picked Up"
- Large blue number showing the count

### Interaction:
**None.** Cards are read-only stat displays — no click targets, no drill-down.

---

## Journey: Resident Dashboard

### Step 1 — Arrive after login
**Route:** `/dashboard/user`
**UI:** 
- Welcome message: "Welcome {username}!"
- A 3-column grid containing one card

### Step 2 — View "Parcel Awaiting Pickup"
**Component:** `UserAwaitingPickup`
**API:** `GET /api/v1/dashboard/user/awaiting-pickup` (assumed)
**UI:** Glass-morphism card (`backdrop-blur-md`) with hover effects:
- "Parcel Awaiting Pickup" label
- Large blue count number
- Hover: scale up slightly, stronger shadow, more opaque background

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No parcel detail drill-down** | High | Dashboard cards are pure stat displays. Clicking a count does nothing — there's no way to see *which* parcels are awaiting pickup from the dashboard. Users must navigate to `/parcel/parcels` separately. |
| 2 | **Admin dashboard only shows 2 metrics** | Medium | No overdue parcels count, no today's check-ins, no locker utilization, no trend data. A dashboard with only two numbers is barely informative. |
| 3 | **Resident dashboard only shows 1 metric** | High | A single number on a full page is extremely sparse. Residents have no way to see recently claimed parcels, recent arrivals, or any timeline of their activity. |
| 4 | **No loading state** | Medium | No skeleton/spinner while API calls resolve. `*ngIf` with `async` pipe shows nothing until data arrives — blank white space. |
| 5 | **No error state** | Medium | If the dashboard API fails, the component simply renders nothing. No error message, no retry button. The `DashboardUser` component even has a comment: `// is error, dont know what to do, maybe show error message`. |
| 6 | **No empty state** | Low | When count = 0, the card still displays "0". No encouraging messaging like "All caught up! No parcels awaiting pickup." |
| 7 | **`DashboardUser` uses wrong layout** | Low | The grid is `grid-cols-3` but only 1 card exists, leaving a 1/3-width card centered. Should be `grid-cols-1` or the card should span full width. |
| 8 | **Admin dashboard route guard is commented out** | Critical | `canActivate: [isLoggedInGuard]` is commented out on the dashboard route. Unauthenticated users could theoretically access `/dashboard`. |

