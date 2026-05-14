# User Story: Resident Unit Verification

**As a** System Administrator / Parcel Room Manager
**I want** to verify that a resident is actually assigned to the unit they claim during registration
**So that** unauthorized users cannot receive parcels for units they do not occupy.

## 📝 Acceptance Criteria
- [ ] Residents must provide a verification code or a valid unit ID during registration.
- [ ] The account remains in a 'Pending Verification' state until an Admin or Manager approves the link.
- [ ] Admin/Manager receives a notification of a new verification request.
- [ ] Approved users are automatically linked to the `ResidentUnit` in the system.
- [ ] Rejected users are notified and must provide alternative proof or contact management.

## 🛠️ Technical Implementation Detail (Learning Path)

### 1. Domain Layer (`ParcelManagement.Core`)
The business logic and rules live here.
- **Entities**: 
    - Add `VerificationStatus` (Enum: Pending, Verified, Rejected) to the `User` entity.
    - Add a `VerificationCode` field to the `ResidentUnit` entity (to be used as a "secret" the resident must provide).
- **Interfaces**:
    - In `IUserService.cs`, add `Task<bool> VerifyUserUnitAsync(Guid userId, Guid residentUnitId)`.
- **Services**:
    - In `UserService.cs`, implement the logic to check if the provided code matches the unit's code and update the `User` status to `Verified`.

### 2. Infrastructure Layer (`ParcelManagement.Infrastructure`)
This is where we talk to the database.
- **Repositories**:
    - In `UserRepository.cs`, you'll need a method to update the `VerificationStatus` of a user.
    - In `ResidentUnitRepository.cs`, add a method to find a unit by its secret `VerificationCode`.
- **Database**:
    - Create a new EF Core migration to add these columns to the `Users` and `ResidentUnits` tables.

### 3. API Layer (`ParcelManagement.Api`)
This exposes the logic to the outside world.
- **DTOs**:
    - Create a `VerifyUnitRequestDto` (contains `UserId`, `UnitId`, and `Code`).
- **Controllers**:
    - In `UserController.cs`, add `[HttpPatch("verify-unit")]`. This endpoint should be protected with `[Authorize(Roles = "Admin, ParcelRoomManager")]`.

---
**Learning Note**: We use the **Service Pattern** here. The Controller only handles the HTTP request; it delegates the "how it's done" to the Service, and the Service delegates the "how it's saved" to the Repository. This is called **Separation of Concerns**.
