# User Story: Unified Locker Check-in Standardization

**As a** Frontend Developer
**I want** a single, standardized endpoint for checking in parcels that includes optional locker assignment
**So that** the check-in user interface remains consistent and doesn't need to switch between v1 and v2 logic based on feature flags.

## 📝 Acceptance Criteria
- [ ] All check-in requests from the frontend use the `v2` Parcel endpoint.
- [ ] The UI provides an optional input for "Locker Name" during check-in.
- [ ] If no locker is assigned, the system handles it as a general room check-in (legacy v1 behavior).
- [ ] The `v1/Parcel/checkIn` endpoint is marked as obsolete/deprecated in the backend documentation.

## 🛠️ Technical Implementation Detail (Learning Path)

### 1. API Layer (`ParcelManagement.Api`)
- **DTO Consolidation**:
    - Ensure `CheckInParcelDto` in V2 allows the `Locker` field to be null or optional.
- **Controller Logic**:
    - In `ParcelController (v2)`, modify the `CheckIn` method:
      ```csharp
      if (string.IsNullOrEmpty(dto.Locker)) {
          // Fallback to v1 logic: check-in without specific locker
          return await _parcelService.CheckInParcelAsync(...);
      } else {
          // Proceed with v2 logic: check-in with locker
          return await _parcelService.CheckInParcelWithLockerAsync(...);
      }
      ```

### 2. Domain Layer (`ParcelManagement.Core`)
- **Service Refactoring**:
    - In `IParcelService`, create a "Unified" check-in method that takes an optional locker ID. This removes the need for the Controller to decide which service method to call.
    - This is called the **Facade Pattern**: you provide a simplified interface (one method) that hides the complexity of the two different check-in modes underneath.

### 3. Frontend Integration (The "Why")
- **State Management**:
    - Instead of having two different API calls in the frontend, the developer now just sends the `CheckInParcelDto` to `/api/v2/Parcel/checkIn`.
    - This reduces the "cyclomatic complexity" of the frontend code (fewer `if/else` blocks in the UI components).

---
**Learning Note**: This is a lesson in **API Evolution**. When you move from v1 to v2, you shouldn't just leave the old version running forever. You "standardize" by making the new version backward-compatible (handling the null locker case) and then deprecating the old one.
