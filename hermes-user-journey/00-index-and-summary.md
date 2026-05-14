# User Journey Documentation — Index & Summary

## About This Document Set

This directory contains detailed user journey documents for every feature in the Parcel Management System frontend (Angular). Each journey documents every screen, form field, interaction, API call, and visual state. Code-level gaps are identified inline with severity ratings.

**Generated from codebase exploration** of `Documents/parcel-management-system/frontend/src/` on the `feature/parcel-overstay-rule` branch.

---

## Journey Files

| # | File | Feature | Pages | Status |
|---|------|---------|-------|--------|
| 01 | `01-authentication.md` | Registration, Login, Logout, Session | `/login`, `/register`, `/registerManager`, `/systemPages/unauthorize` | Functional |
| 02 | `02-dashboard.md` | Admin & Resident Dashboards | `/dashboard/user` | Functional (sparse) |
| 03 | `03-parcel-check-in.md` | Single & Bulk Parcel Check-In | `/parcel/checkIn` | Functional (gaps in feedback) |
| 04 | `04-parcel-claim.md` | Single & Bulk Parcel Claim | `/parcel/claim` | Functional (gaps in feedback) |
| 05 | `05-parcel-tracking.md` | Parcel Search & Tracking History | `/parcel/tracking`, `/parcel/tracking/searchResult` | Functional |
| 06 | `06-parcel-listing.md` | All Parcels Table (Search, Filter, Paginate) | `/parcel/parcels` | Functional |
| 07 | `07-locker-management.md` | Locker CRUD | `/locker`, `/locker/addEdit`, `/locker/addEdit/:id` | Functional |
| 08 | `08-resident-unit-management.md` | Resident Unit CRUD | `/residentUnit/units`, `/residentUnit/units/edit`, `/residentUnit/units/edit/:id` | Functional |
| 09 | `09-user-resident-unit-assignment.md` | User-Unit Assignment | `/resident/userResidentUnit` | **STUB — Not Implemented** |
| 10 | `10-notification-preferences.md` | Notification Channel & Event Toggles, Quiet Hours | `/settings/notifications` | Functional |
| 11 | `11-theme-and-navigation.md` | Sidebar, Topbar, Dark Mode, Role-Based Nav | All authenticated pages | Functional (gaps in mobile) |

---

## System Architecture at a Glance

### User Roles

| Role | Can Access |
|------|-----------|
| **Resident** | Dashboard (own), Tracking, Claim, All Parcels, Notification Settings |
| **ParcelRoomManager** | Dashboard, Check-In, Tracking, All Parcels, Resident Units, Lockers, User-Unit Assignment |
| **Admin** | Everything (Dashboard, Check-In, Tracking, Claim, All Parcels, Resident Units, Lockers, User-Unit Assignment) |

### Tech Stack (Frontend)
- **Framework:** Angular 20 (standalone components, `@let` syntax, `@if`/`@for` control flow)
- **Styling:** Tailwind CSS with CSS custom properties for theming
- **State:** RxJS (BehaviorSubject + combineLatest streams)
- **HTTP:** Angular HttpClient with JWT interceptor + refresh token rotation
- **Forms:** Reactive Forms (FormBuilder, FormGroup, Validators)
- **Routing:** Lazy-loaded feature modules with role-based guards

### Common Components Library
- `my-button` — Primary/danger variants, loading state
- `my-form` — Config-driven form generation from `FormFieldConfig[]`
- `my-table` — Generic table with columns, pagination, action columns
- `my-searchbar` — Search input with debounced emission
- `my-switch` — Toggle switch with label
- `file-upload` — Excel/CSV upload with mapper callback
- `pagination` — Page navigation controls

---

## Critical Gaps Summary

| Priority | Gap | Affected Feature |
|----------|-----|------------------|
| **P0** | User-Resident Unit Assignment page is a hardcoded placeholder | 09-user-resident-unit-assignment |
| **P0** | No mobile responsive sidebar — unusable on phones | 11-theme-and-navigation |
| **P0** | Admin dashboard route guard commented out | 02-dashboard |
| **P0** | No bulk check-in/claim success/failure feedback | 03-check-in, 04-claim |
| **P1** | No "Forgot Password" flow | 01-authentication |
| **P1** | Dashboard: no parcel drill-down, too few metrics | 02-dashboard |
| **P1** | No delete for Lockers or Resident Units | 07-locker, 08-units |
| **P1** | No date columns or sorting in parcels table | 06-parcel-listing |
| **P1** | Tracking history lacks visual timeline | 05-tracking |
| **P1** | No update-failure feedback on notification prefs | 10-notifications |
| **P2** | No debounce on search inputs | 06-listing, 07-locker, 08-units |
| **P2** | No form reset after successful check-in | 03-check-in |
| **P2** | No unsaved changes warning | 04-claim, 10-notifications |
| **P2** | Register Manager page has no navigation link from login | 01-authentication |
| **P2** | No user profile/avatar in topbar | 11-navigation |
| **P3** | Dead code: Navbar and Sidebar components unused | 11-navigation |
| **P3** | Inconsistent route naming (addEdit vs edit) | 07-locker, 08-units |
| **P3** | No bulk import for units | 08-units |
| **P3** | ParcelRoomManager can't claim parcels | 04-claim |

---

## How to Use These Documents

1. **For UI redesign:** Use these journeys as the complete specification of what each page currently does. Any redesign should preserve or improve each step in the journey.

2. **For gap remediation:** The "Current Gaps" tables in each document are prioritized (Critical > High > Medium > Low). Start with P0 items.

3. **For testing:** Each journey describes expected behavior end-to-end. Use as test case specifications.

4. **For onboarding:** New developers can read these to understand the entire system without exploring the codebase manually.

