# User Journey: Authentication

## Overview
The authentication system supports three user roles: **Resident**, **ParcelRoomManager**, and **Admin**. Users can register as a Resident (self-service), register as a Manager (admin-only or open registration), and log in with email/username + password. JWT tokens with refresh-token rotation secure the session.

## Actors
- **Unauthenticated Visitor** — anyone landing on the app
- **Resident** — a building resident registering or logging in
- **ParcelRoomManager** — a staff member registering or logging in
- **Admin** — system administrator logging in

## Journey: Resident Registration

### Step 1 — Navigate to registration
**Trigger:** Visitor clicks "Register" link on the login page.
**Route:** `/register`
**UI:** The Register page loads — a centered card on a dark/gray background.

| Field | Type | Validation |
|-------|------|------------|
| Email | text (email) | required, valid email format |
| Username | text | required, max 10 characters |
| Resident Unit | text | required, max 10 characters (default: "RU001") |
| Password | password | required |
| Confirm Password | password | required, must match password |
| Agree Terms | checkbox | must be checked to enable the Register button |

### Step 2 — Fill in the form
**Action:** Visitor enters email, username, resident unit number, password, confirms password, and checks the terms checkbox.
**System behavior:** The Register button stays disabled until the terms checkbox is checked. Password mismatch shows inline error: "Passwords do not match".

### Step 3 — Submit registration
**Action:** Visitor clicks "Register".
**System behavior:**
- Button shows loading spinner
- `POST /api/v1/auth/register` with payload: `{ Username, Email, ResidentUnit, Password }`
- **Success:** Redirects to `/login`
- **Failure:** Error message displayed below the form in red text (e.g., "Username already taken", "Email already registered")

### Post-condition:
User account is created with role **Resident**. They must now log in.

---

## Journey: Manager Registration

### Step 1 — Navigate
**Route:** `/registerManager`
**UI:** A centered white card with "Register as Manager" heading.

| Field | Type | Validation |
|-------|------|------------|
| Username | text | required |
| Email | email | required |
| Password | password | required |
| Confirm Password | password | required, must match |

### Step 2 — Fill and submit
**Action:** User fills in all fields and clicks "Register As Manager".
**System behavior:**
- `POST /api/v1/auth/registerManager` with payload: `{ Username, Email, Password }`
- **Success:** Redirects to `/login`
- **Failure:** Error message shown in red

### Post-condition:
User account created with role **ParcelRoomManager**.

---

## Journey: Login

### Step 1 — Navigate
**Route:** `/login` (default landing page)
**UI:** Centered card on dark background with "Login" heading.

| Field | Type | Validation |
|-------|------|------------|
| Email or Username | text | required |
| Password | password | required |

### Step 2 — Enter credentials
**Action:** User enters email/username and password.
**System behavior:** Login button enabled only when both fields are valid (non-empty).

### Step 3 — Submit login
**Action:** User clicks "Login".
**System behavior:**
- Button shows loading spinner
- `POST /api/v1/auth/login` with `{ Username, PlainPassword }`
- **Success (200):** Access token saved to localStorage. Role fetched from `/api/v1/user/me` endpoint. Redirects to `/dashboard/user`.
- **Success with `returnUrl` query param:** Redirects to the saved URL instead.
- **Failure (401/400):** Error message shown below the button in red: "Invalid username or password"

### Post-condition:
- JWT access token stored in localStorage under key `parcel-management-system-token`
- User role cached in memory/RoleStorage for 60 minutes
- Auth interceptor attaches `Authorization: Bearer <token>` to all API requests
- Expired tokens auto-refresh via `POST /api/v1/auth/refresh` (withCredentials: true)

---

## Journey: Logout

### Trigger:
User clicks "Logout" button in the topbar.

### System behavior:
1. Role cleared from RoleStorage/memory
2. JWT token removed from localStorage
3. Redirected to `/login`

---

## Journey: Unauthorized Access

### Trigger:
A logged-in user tries to access a route not permitted for their role (guarded by `canActivate: [isLoggedInGuard, isAdminAndManagerAuthed]` or similar).

### System behavior:
**Route:** `/systemPages/unauthorize`
**UI:** Simple "Unauthorized" page explaining the user doesn't have permission.

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No password strength rules** | Medium | Password field only requires `Validators.required` — no minimum length (backend may enforce 6 chars per placeholder text), no complexity requirements. Weak passwords could be accepted. |
| 2 | **Login form uses `emailUsername` but sends only `Username`** | Low | The field is labeled "Email or Username" but the payload always sends it as `Username`. Backend must handle both; if not, users trying to log in with email will fail. |
| 3 | **Register Manager page has no link from login** | Medium | There's no navigation path from `/login` to `/registerManager`. Users have to know the URL. The login page only links to `/register` (Resident registration). |
| 4 | **No "Forgot Password" flow** | High | No password reset mechanism exists in the frontend or visible auth routes. |
| 5 | **Register page default Resident Unit** | Low | Defaults to "RU001" which may not be a valid unit, causing confusion. |
| 6 | **No session timeout UI warning** | Medium | When the refresh token expires, the user is silently logged out and redirected without warning. |

