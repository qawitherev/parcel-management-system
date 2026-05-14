# User Journey Plan - Parcel Management System

This document outlines the user journeys for the Parcel Management System based on the current backend endpoints.

## 👥 Roles & Responsibilities
- **Resident**: A user living in a unit who receives parcels.
- **ParcelRoomManager**: Staff responsible for checking in parcels and managing the parcel room.
- **Admin**: System administrator with overarching management capabilities.

---

## 🛤️ User Journeys

### 1. Resident Journey
**Goal**: Register, track, and claim parcels.

- **Onboarding**: 
  - Register as a resident $\rightarrow$ `POST /api/v1/User/register/resident`
  - Login to the system $\rightarrow$ `POST /api/v1/User/login`
- **Parcel Tracking**:
  - View all my parcels (delivered/claimed) $\rightarrow$ `GET /api/v1/Parcel/myParcels`
  - Track a specific parcel via tracking number $\rightarrow$ `GET /api/v1/Parcel/trackingNumber/{number}`
  - View detailed history of a parcel's events $\rightarrow$ `GET /api/v1/Parcel/trackingNumber/{number}/history`
- **Claiming Parcels**:
  - Claim a parcel using a tracking number $\rightarrow$ `POST /api/v1/Parcel/trackingNumber/{number}/claim`
  - Bulk claim multiple parcels $\rightarrow$ `POST /api/v1/Parcel/bulkClaim`

### 2. ParcelRoomManager Journey
**Goal**: Efficiently manage the intake and release of parcels.

- **Inventory Management**:
  - Check in a new parcel (Single) $\rightarrow$ `POST /api/v1/Parcel/checkIn` (or v2 for locker support)
  - Check in multiple parcels at once $\rightarrow$ `POST /api/v1/Parcel/bulkCheckIn`
  - View parcels awaiting pickup $\rightarrow$ `GET /api/v1/Parcel/awaitingPickup`
  - View recently picked up parcels $\rightarrow$ `GET /api/v1/Parcel/recentlyPickedUp`
- **Parcel Operations**:
  - Search/Filter all parcels for oversight $\rightarrow$ `GET /api/v1/Parcel/all`
  - Add a manual tracking event (e.g., "Moved to Shelf B") $\rightarrow$ `POST /api/v1/Parcel/trackingNumber/{number}/events`
  - Help a resident claim a parcel $\rightarrow$ `POST /api/v1/Parcel/trackingNumber/{number}/claim`
- **Facility & Resident Management**:
  - Manage lockers (Create/Update/List) $\rightarrow$ `POST/PATCH/GET /api/v1/Locker`
  - Manage Resident Units $\rightarrow$ `POST/PATCH/GET /api/v1/ResidentUnit`
  - Assign users to a unit $\rightarrow$ `POST /api/v1/ResidentUnit/addUser`
  - Update residents associated with a unit $\rightarrow$ `PATCH /api/v1/UserResidentUnit`

### 3. Admin Journey
**Goal**: System configuration and high-level oversight.

- **User Management**:
  - Register new Parcel Room Managers $\rightarrow$ `POST /api/v1/User/register/parcelRoomManager`
  - Overview of all users in the system $\rightarrow$ `GET /api/v1/User`
- **System Oversight**:
  - Access all parcel data $\rightarrow$ `GET /api/v1/Parcel/all`
  - Manage lockers and units (same as ParcelRoomManager)

---

## ⚠️ Gaps & Discrepancies

### 1. Resident Unit Association
- **Issue**: Residents can register themselves via `POST /api/v1/User/register/resident`, but there is no clear endpoint for a resident to *change* their unit or for an admin to *verify* the unit they claim during registration.
- **Suggested Solution**: Implement a "Unit Verification" flow where residents must enter a unique unit code or wait for Admin/Manager approval before being linked to a `ResidentUnit`.

### 2. Notification System
- **Issue**: While `NotificationPrefController` exists in the files, there are no endpoints for *sending* notifications or *triggering* a "Parcel Arrived" alert to the resident.
- **Suggested Solution**: Add a webhook or service-level trigger in `CheckInParcel` that automatically calls a notification service to email/SMS the resident.

### 3. Parcel Checkout/Return
- **Issue**: The system allows "Claiming" (likely marking as picked up), but there is no formal "Return to Sender" or "Redirect" journey.
- **Suggested Solution**: Implement a `POST /api/v1/Parcel/trackingNumber/{number}/return` endpoint for Managers to handle undeliverable parcels.

### 4. Locker Logic (v1 vs v2)
- **Issue**: `v1` check-in doesn't explicitly handle locker assignment in the DTO, whereas `v2` does. This creates a discrepancy in how the UI should handle the check-in flow.
- **Suggested Solution**: Deprecate `v1/Parcel/checkIn` in the frontend and standardize on `v2` to ensure locker mapping is always captured.
