# User Journey: Parcel Check-In

## Overview
Parcel Check-In is how staff (ParcelRoomManager/Admin) registers a new parcel into the system when it arrives at the parcel room. There are two modes: **single check-in** via a form and **bulk check-in** via Excel/CSV file upload.

## Actors
- **ParcelRoomManager** — primary user; checks in parcels daily
- **Admin** — can also check in parcels

---

## Journey: Single Parcel Check-In

### Step 1 — Navigate
**Route:** `/parcel/checkIn`
**Sidebar path:** Parcel > CheckIn (visible only to `ParcelRoomManager`)
**UI:** Page loads with:
- "Bulk Check In" button (top-right, primary style)
- A dynamic form (`app-my-form`) with 5 fields and a "Check In Parcel" submit button

| Field | Type | Placeholder | Validation |
|-------|------|-------------|------------|
| Tracking Number | text | "Enter tracking number" | required, max 20 chars |
| Resident Unit | text | "Enter resident unit" | required, max 20 chars |
| Locker | text | "Enter locker" | required, max 20 chars |
| Weight | text | "Enter weight" | required |
| Dimension | text | "Enter dimension" | optional |

### Step 2 — Fill form
**Action:** Manager enters tracking number from the package label, selects/enters the resident unit, chooses which locker the parcel is placed in, and optionally enters weight and dimensions.

### Step 3 — Submit check-in
**Action:** Manager clicks "Check In Parcel".
**System behavior:**
- Button shows loading spinner (`isCheckingIn = true`)
- `POST /api/v1/parcel/checkIn` with `CheckInPayload`
- **Success:** Response is emitted via `checkInResponse$` observable. However, there is **no visual success message** in the template — only error messages are shown.
- **Failure:** Red error text below the form: `{{ checkInResponse.message }}`

### Step 4 — (Post-condition)
- Parcel is registered with status **AwaitingPickup**
- A tracking event is created: "Parcel checked in"
- The resident associated with the unit receives a notification (if their notification preferences have "Parcel Arrival" enabled)

---

## Journey: Bulk Check-In

### Step 1 — Open bulk modal
**Action:** Manager clicks "Bulk Check In" button (top-right).
**System behavior:** `isBulkCheckInPopup` toggles to `true`. A file upload component (`app-file-upload`) appears.

### Step 2 — Upload file
**Action:** Manager selects an Excel/CSV file containing multiple check-in records.
**System behavior:**
- The file is parsed using `mapperCheckInPayload` (from `core/bulk-action/excel-to-json`)
- Each row becomes a `CheckInPayload` object
- Data is emitted via `dataEmitter` to `onUploadFinished(data)`

### Step 3 — Process bulk check-in
**System behavior:**
- `POST /api/v1/parcel/bulkCheckIn` with array of `CheckInPayload`
- Response logged to console (`AppConsole.log`) but **no user-facing success/error feedback is shown** after bulk upload

### Step 4 — Close modal
**Action:** Click "Bulk Check In" again (toggles popup off) or click Cancel on the file upload.
**Gap:** The form below remains populated/visible during bulk process. After bulk upload, there's no visual confirmation of how many succeeded/failed.

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No success feedback for single check-in** | High | The template checks `if (checkInResponse && 'error' in checkInResponse)` for errors but never shows a success message. Manager doesn't know if the check-in worked unless they navigate to the parcels list and verify. |
| 2 | **No bulk result feedback** | Critical | `onUploadFinished` calls `bulkCheckInParcel` and logs to console but never binds the result to a template variable. The manager has zero visibility into: how many succeeded, how many failed, which rows had errors. |
| 3 | **Form doesn't reset after success** | Medium | After a successful check-in, the form still shows the previous values. Manager must manually clear fields for the next parcel. |
| 4 | **No locker/resident unit autocomplete** | Medium | Fields are plain text inputs. The manager must type locker names and resident units from memory — no dropdown, no typeahead search against existing lockers/units. Prone to typos. |
| 5 | **Weight and Dimension are free-text** | Low | No unit labels (kg? lbs? cm? inches?), no numeric validation. Could enter "heavy" and the system would accept it. |
| 6 | **No duplicate tracking number prevention UI** | Medium | If the same tracking number is checked in twice, the error comes from the backend. No client-side dedup warning before submission. |
| 7 | **No bulk file template download** | Low | No link to download a template CSV/Excel file showing the expected format. First-time users must guess the columns. |
| 8 | **Bulk modal doesn't block form interaction** | Low | When the file upload popup is open, the regular check-in form is still active below it. Could lead to confusion. |

