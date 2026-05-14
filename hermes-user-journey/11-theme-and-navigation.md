# User Journey: Theme, Navigation & Layout

## Overview
The application uses a sidebar + topbar layout for authenticated pages. Navigation is role-aware, showing or hiding menu items based on user permissions. A dark/light theme toggle is available in the topbar.

## Actors
- **All authenticated users** — interact with navigation and theming

---

## Journey: Navigate via Sidebar

### Step 1 — See the sidebar
**Location:** Left side of screen, fixed position (`fixed inset-y-0 left-0`), 256px wide (`w-64`).
**Always visible on:** All routes under `NormalLayout` (authenticated pages).

**Header:** "Parcel App" in bold (`text-2xl font-bold`) with a bottom border.

### Step 2 — View role-filtered menu
**Menu structure (defined in `NormalLayout.ts` as `MENU_ITEMS`):**

| Parent | Roles | Children | Child Routes | Child Roles |
|--------|-------|----------|-------------|-------------|
| Dashboard | Resident, Manager, Admin | (none, direct link) | `/dashboard/user` | — |
| Parcel | Resident, Manager, Admin | CheckIn | `/parcel/checkIn` | Manager |
| | | Tracking | `/parcel/tracking` | Resident, Manager, Admin |
| | | Claim | `/parcel/claim` | Resident, Admin |
| | | All Parcels | `/parcel/parcels` | Resident, Manager, Admin |
| Resident | Manager, Admin | User Resident Unit | `/resident/userResidentUnit` | Manager, Admin |
| Resident Unit | Manager, Admin | Resident Units | `/residentUnit/units` | Manager, Admin |
| Locker | Manager, Admin | Lockers | `/locker` | Manager, Admin |
| Settings | Resident | Notifications | `/settings/notifications` | Resident |

**Behavior:**
- Parents with children act as accordion toggles: clicking expands/collapses child items
- Expansion state is persisted via `SidebarService` (stored as `Map<string, boolean>` in memory)
- Each child item is also role-filtered: a Resident sees "Tracking", "Claim", "All Parcels" under Parcel, but NOT "CheckIn"

### Step 3 — Click a menu item
**Action:** Click a route link.
**System behavior:** Angular router navigates. The active page title appears in the topbar.

---

## Journey: Use the Topbar

### Location:
Sticky header above the main content area (`shadow p-4`).

### Elements (left to right):
1. **Page title** — dynamic, driven by route `data.title` via `LayoutService.pageTitle$`
2. **Dark Mode toggle** — `app-my-switch` with label "Dark Mode"
3. **Logout button** — `app-my-button` variant "danger", red styling

### Page title examples:
| Route | Title |
|-------|-------|
| `/parcel/tracking` | "Parcel Tracking" |
| `/parcel/checkIn` | "Check In" |
| `/parcel/claim` | "Parcel Claim" |
| `/parcel/parcels` | "All Parcels" |
| `/dashboard/user` | "Dashboard" |
| `/residentUnit/units` | "Resident Units" |
| `/locker` | "Locker" |
| `/settings/notifications` | "Notifications" |

---

## Journey: Toggle Dark Mode

### Step 1 — Flip the switch
**Action:** User clicks the Dark Mode toggle in the topbar.
**System behavior:**
- `ThemeService.toggleMode()` called
- `ThemeService` manages a CSS class toggle and persists preference (likely in localStorage)
- Theme CSS variables (`--clr-surface-a0`, `--clr-dark-a0`, etc.) change reactively

### Visual states:
- **Light mode:** `isDarkMode = false` — switch shows "Dark Mode" label in off state
- **Dark mode:** `isDarkMode = true` — switch shows "Dark Mode" label in on state

---

## Journey: Logout

### Step 1 — Click Logout
**Action:** User clicks the red "Logout" button in the topbar.
**System behavior:**
1. `AuthService.logout()`:
   - Clears role from `RoleStorage`
   - Removes JWT from `localStorage` (key: `parcel-management-system-token`)
   - Navigates to `/login`

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **Navbar component is unused** | Low | A `Navbar` component exists at `common/components/navbar/` but only contains `<p>navbar works!</p>`. It's imported but never used. The actual sidebar is built directly in `NormalLayout`. The `Navbar` and `Sidebar` components (also unused, separate file) are dead code. |
| 2 | **Sidebar component is unused** | Low | `Sidebar` component at `common/components/sidebar/` has a hardcoded static sidebar (with "Parent Item", "Child Item 1/2") but is never instantiated. The working sidebar lives inline in `NormalLayout`. |
| 3 | **No mobile responsive sidebar** | High | The sidebar is fixed at `w-64` with no hamburger toggle. On mobile screens, it permanently occupies 256px of the 320-375px viewport, leaving almost no room for content. There's a `transform transition-transform duration-300` class and a commented-out hamburger, suggesting responsive behavior was planned but never completed. |
| 4 | **No active route highlighting** | Medium | The sidebar doesn't highlight which menu item/page is currently active. Users have no visual indicator of where they are in the navigation tree. |
| 5 | **Sidebar state lost on refresh** | Medium | `SidebarService` stores expanded state in memory (`Map`). On page refresh, all menu groups collapse. Should persist to `localStorage`. |
| 6 | **Page title only from route data** | Low | Pages without `data: { title: '...' }` in routing config show no title. The title doesn't update for dynamic pages (e.g., "Edit Locker: Locker A" vs just "Locker"). |
| 7 | **No user avatar/name in topbar** | Low | The topbar only shows toggle + logout. No "Logged in as John" indicator, no profile link. The resident dashboard shows the username but only on that specific page. |
| 8 | **Dark mode toggle label is always "Dark Mode"** | Low | The switch label says "Dark Mode" in both states. It should read "Dark Mode" when off and "Light Mode" when on (or be an icon like sun/moon). |

