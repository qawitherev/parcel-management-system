# User Story: Return to Sender Flow

**As a** Parcel Room Manager
**I want** to be able to mark a parcel as "Returned to Sender"
**So that** the system reflects the actual physical location of the parcel and clears it from the "Awaiting Pickup" list.

## 📝 Acceptance Criteria
- [ ] Managers can select a parcel from the "Awaiting Pickup" list and trigger a "Return to Sender" action.
- [ ] The parcel status is updated to `Returned` (or similar).
- [ ] A manual tracking event is automatically created: "Parcel returned to sender".
- [ ] The resident is notified that their parcel was returned due to non-pickup.

## 🛠️ Technical Implementation Detail (Learning Path)

### 1. Domain Layer (`ParcelManagement.Core`)
- **Model Updates**: 
    - Update the `ParcelStatus` enum in `ParcelManagement.Core.Model` to include `Returned`.
- **Service Logic**:
    - In `IParcelService.cs`, add `Task ReturnParcelAsync(string trackingNumber, Guid managerId)`.
    - Implementation in `ParcelService.cs`:
        1. Fetch the parcel by tracking number.
        2. Change status to `ParcelStatus.Returned`.
        3. Call `ITrackingEventService.ManualEventTracking(...)` to add the history record.
        4. Trigger the `NotificationService` (from the notifications user story).

### 2. Infrastructure Layer (`ParcelManagement.Infrastructure`)
- **Repository**:
    - The existing `ParcelRepository.UpdateParcelAsync` is sufficient here because it uses EF Core's `SetValues`, which updates the entity properties based on the updated object passed from the service.

### 3. API Layer (`ParcelManagement.Api`)
- **Endpoint**:
    - In `ParcelController.cs`, add:
      `[HttpPost("trackingNumber/{trackingNumber}/return")]`
      `[Authorize(Roles = "ParcelRoomManager")]`
    - The controller should call `_parcelService.ReturnParcelAsync`.

---
**Learning Note**: Notice how the "Return" flow re-uses the **Tracking Event Service**. This is the **DRY (Don't Repeat Yourself)** principle. Instead of writing new logic to record the history, we use the existing generic manual event tool.
