# User Journey: Parcel Claim

## Overview
Parcel Claim allows a resident (or admin on their behalf) to mark a parcel as picked up from the parcel room. Like Check-In, it supports **single claim** via tracking number and **bulk claim** via file upload.

## Actors
- **Resident** — claims their own parcels
- **Admin** — can claim any parcel on behalf of residents
- **ParcelRoomManager** — notably does NOT have the Claim menu item in the sidebar

---

## Journey: Single Parcel Claim

### Step 1 — Navigate
**Route:** `/parcel/claim`
**Sidebar path:** Parcel > Claim (visible to `Resident`, `Admin`)
**UI:** Page loads with:
- "Bulk Claim" button (top-right, primary style)
- A form with a single input and "Claim Parcel" button

| Field | Type | Placeholder | Validation |
|-------|------|-------------|------------|
| Tracking Number | text | "Enter tracking number" | required, max 20 chars |

### Step 2 — Enter tracking number
**Action:** Resident enters the tracking number from their pickup notification or package label.

### Step 3 — Submit claim
**Action:** Resident clicks "Claim Parcel".
**System behavior:**
- `POST /api/v1/parcel/claim` (or similar endpoint) with `{ trackingNumber }`
- Form resets immediately after submission (even if call hasn't completed)
- **Success:** Green success message shown: `{{ claimResponse.message }}`
- **Failure:** Red error message shown: `{{ claimResponse.message }}`

### Example success message:
> "Parcel claimed successfully"

### Example error messages:
> "Parcel not found"
> "Parcel already claimed"
> "You are not authorized to claim this parcel"

### Step 4 — (Post-condition)
- Parcel status changes from **AwaitingPickup** to **Claimed**
- A tracking event is created: "Parcel claimed by {username}"
- The resident receives a claim confirmation notification (if "Parcel Claim" event is enabled in their preferences)

---

## Journey: Bulk Claim

### Step 1 — Open bulk modal
**Action:** User clicks "Bulk Claim" button.
**System behavior:** `isBulkClaimPopup` toggles to `true`. File upload component appears.

### Step 2 — Upload file
**Action:** User selects an Excel/CSV file containing tracking numbers (one per row).
**System behavior:**
- Parsed via `mapperClaimPayload` (from `core/bulk-action/excel-to-json`)
- Each row becomes a `ClaimPayload` with a tracking number
- Data emitted to `onBulkClaim(trackingNumbers)`

### Step 3 — Process bulk claim
**System behavior:**
- `claimService.bulkClaim(trackingNumbers)` is called
- **Gap:** The returned observable is never assigned to a template variable. No success/error feedback is shown to the user. The result is completely invisible.

---

## Current Gaps (Code-Level)

| # | Gap | Severity | Description |
|---|-----|----------|-------------|
| 1 | **No bulk claim feedback** | Critical | Same issue as bulk check-in. `onBulkClaim()` calls the service but the observable result is never displayed. Users have no idea which claims succeeded or failed. |
| 2 | **Form resets before response** | Medium | `this.formGroup.reset()` is called immediately after `claimParcel()`, before the API call completes. If the claim fails, the user has to re-type the tracking number. |
| 3 | **No confirmation dialog** | Medium | Single-click claims the parcel instantly. No "Are you sure?" confirmation, no preview of which parcel (resident name, arrival date) is being claimed. Risk of claiming the wrong parcel. |
| 4 | **No parcel preview before claim** | Medium | User only sees a tracking number input. There's no lookup to show "This parcel is for Unit 5B, arrived on Jan 15" before the user confirms the claim. |
| 5 | **ParcelRoomManager can't claim** | Medium | The Claim menu item is restricted to `Resident` and `Admin` roles. If a resident sends someone else to pick up their parcel, the manager on duty cannot process the claim through the UI. |
| 6 | **No claim history for resident** | Low | After claiming, there's no immediate "here's what you just claimed" summary. User must navigate to Tracking or All Parcels to verify. |
| 7 | **Bulk file format same as check-in** | Low | The mapper is `mapperClaimPayload` but without documentation, it's unclear if the bulk claim file needs only tracking numbers or other fields. |
| 8 | **No in-app notification after claim** | Low | Success message appears inline but disappears on navigation. No toast/notification that persists. |

