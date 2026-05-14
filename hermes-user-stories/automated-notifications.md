# User Story: Automated Parcel Notifications

**As a** Resident
**I want** to receive an immediate notification when my parcel is checked into the parcel room
**So that** I know exactly when my package is ready for pickup without checking the app manually.

## 📝 Acceptance Criteria
- [ ] Upon a successful `checkIn` (v1 or v2), the system identifies the resident(s) linked to the target unit.
- [ ] A notification is triggered based on the resident's `NotificationPreference`.
- [ ] The notification contains the tracking number and the locker name (if applicable).
- [ ] Residents can opt-out of specific notification types via the `NotificationPrefController`.

## 🛠️ Technical Implementation Detail (Learning Path)

### 1. Domain Layer (`ParcelManagement.Core`)
- **New Service**: Create `INotificationService.cs` and its implementation `NotificationService.cs`. 
    - This service should handle the logic of *what* message to send.
- **Events**: 
    - To keep the system decoupled, use the **Observer Pattern** or an **Event Bus**. Instead of the `ParcelService` calling the `NotificationService` directly, it should trigger a `ParcelArrivedEvent`.

### 2. Infrastructure Layer (`ParcelManagement.Infrastructure`)
- **External Adapters**: 
    - Create a `NotificationAdapter` that implements `INotificationService`. This adapter will actually call external APIs like SendGrid (Email) or Twilio (SMS).
    - This prevents your core business logic from being "locked in" to one provider. If you switch from Twilio to AWS SNS, you only change the Adapter, not the Business Logic.

### 3. API Layer (`ParcelManagement.Api`)
- **Integration Point**:
    - In `ParcelController.cs` (the `checkIn` methods), after `_parcelService.CheckInParcelAsync(...)` returns successfully, the `NotificationService` should be triggered.
- **Preferences**:
    - Use the existing `NotificationPrefController.cs` to ensure the service checks `User.NotificationPreference` before sending.

---
**Learning Note**: This implementation uses **Dependency Inversion**. The Core layer defines *what* a notification service does (`INotificationService`), and the Infrastructure layer defines *how* it's actually sent (via a specific API). This makes the code highly testable and flexible.
