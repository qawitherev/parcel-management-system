# User Journey: Notification Preferences

## Overview
Each resident can configure how and when they receive notifications about parcel activity. The preferences control both **channels** (Email, WhatsApp) and **trigger events** (check-in, claim, overdue). Quiet Hours allow muting notifications during sleeping hours.

## Actors
- **Resident** — configures their own preferences
- **ParcelRoomManager / Admin** — no access (Settings menu only visible to Residents in sidebar)

---

## Journey: Configure Notification Preferences

### Step 1 — Navigate
**Route:** `/settings/notifications`
**Sidebar path:** Settings > Notifications (visible only to `Resident`)
**UI:** Page with three card sections:

---

### Section A: Primary Channel

**Heading:** Bell icon + "Notification Preferences" title
**Card 1:** "Primary Channel" — rounded card with two toggle rows:

| Setting | Toggle | Default |
|---------|--------|---------|
| Email Notifications | `app-my-switch` | (from API) |
| WhatsApp Notifications | `app-my-switch` | (from API) |

**Behavior:** Tapping a switch instantly flips the visual state (optimistic UI). No save happens until "Apply" is clicked.

---

### Section B: Event Preferences

**Card 2:** "Event Preferences" — three toggle rows:

| Setting | Toggle | Description |
|---------|--------|-------------|
| Parcel Arrival | `app-my-switch` | Notify when a new parcel is checked in for your unit |
| Parcel Claim | `app-my-switch` | Notify when your parcel is claimed/picked up |
| Parcel Overdue | `app-my-switch` | Notify when a parcel has been waiting too long |

**Behavior:** Same optimistic toggle pattern. Changes are local until Apply.

---

### Section C: Quiet Hours

**Card 3:** "Quiet Hours" — header row with a toggle:

| Setting | Type | Description |
|---------|------|-------------|
| Quiet Hours toggle | `app-my-switch` | Enable/disable quiet hours entirely |

When **disabled** (default if `quietHoursFrom`/`quietHoursTo` are null from API):
- The "From" and "To" time inputs are **disabled** (grayed out)
- Apply button is enabled

When **enabled**:
- Two time input fields become active:

| Field | Type | Binding |
|-------|------|---------|
| From | `time` input | `prefState.quietHoursFrom` |
| To | `time` input | `prefState.quietHoursTo` |

- **Validation:** Apply button is disabled if quiet hours are enabled but From/To are null. (`isCanApply` getter: `!isQuietHoursEnabled || (enabled && from != null && to != null)`)

---

### Step 2 — Toggle preferences
**Action:** User flips switches for channels and events. User optionally sets quiet hours.
**System behavior:**
- All changes are stored in `prefState` object (local component state, cloned from API response)
- Visual state updates immediately
- No API calls made yet

### Step 3 — Apply changes
**Action:** User clicks "Apply" button (only enabled when `isCanApply` is true).
**System behavior:**
- Button shows loading spinner (`isLoading = true`)
- Payload constructed:
  ```
  {
    isEmailActive, isWhatsAppActive,
    isOnCheckInActive, isOnClaimActive, isOverdueActive,
    quietHoursFrom, quietHoursTo  // null if quiet hours disabled
  }
  ```
- `PUT /api/v1/notificationPrefs/{id}` called via `npService.updateNotificationPref()`
- **Success:** Green text appears: "Preferences Saved!" below the Apply button
- **Failure:** Error is handled by the API interceptor (no inline error display for update failures in the template)

### Step 4 — (Initial load)
**On page load (`ngOnInit`):**
- `npService.getNotificationPrefByUser()` called
- Response mapped to `prefState`
- `isQuietHoursEnabled` derived: `true` if both `quietHoursFrom` and `quietHoursTo` are non-null, `false` otherwise

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No update failure feedback** | High | If the PUT request fails, no error message is displayed in the template. The `notificationPrefUpdateResponse$` is used only for the success check (`'success' in createUpdateResponse`). API errors caught by the interceptor may show a generic message but not inline. |
| 2 | **No unsaved changes warning** | Medium | If the user toggles several switches and navigates away without clicking Apply, all changes are silently lost. No "You have unsaved changes" guard. |
| 3 | **WhatsApp channel has no configuration UI** | Medium | The WhatsApp toggle exists but there's no field to enter a WhatsApp phone number. Backend must have it pre-configured or use the user's phone from their profile. |
| 4 | **No email address display** | Low | The email channel toggle doesn't show which email address notifications will be sent to. If the user has multiple emails or wants to verify, there's no feedback. |
| 5 | **Quiet Hours validation is basic** | Low | No validation that "To" is after "From" (e.g., From 10:00 PM, To 06:00 AM — a valid overnight range, but From 10:00 PM, To 09:00 PM would also pass and be nonsensical). |
| 6 | **No test notification** | Low | No "Send test notification" button to verify that notifications are working. Users must wait for an actual parcel event to confirm their settings. |
| 7 | **Loading state not shown on initial fetch** | Medium | While `notificationPref$` is loading, the template shows nothing (the `@if` block requires `'id' in notificationPref`). No spinner or skeleton. |
| 8 | **Settings menu only visible to Resident** | Medium | Admin/Manager have no access to notification settings. They might want to configure system-wide notification behavior or troubleshoot a resident's preferences. |

