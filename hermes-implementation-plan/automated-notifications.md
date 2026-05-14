# Implementation Plan: Automated Parcel Notifications

> **User Story #1** — As a Resident, I want an immediate notification when my parcel is checked into the parcel room.

---

## 1. Architecture Decision

### Approach: Enqueuer + BackgroundService (fire-and-forget via Channel)

The notification dispatch runs **asynchronously after check-in completes**, mirroring the proven `ParcelOverstayBackgroundService` pattern already in the codebase. This decouples the check-in flow from potentially slow external API calls (SendGrid, Twilio). The check-in returns instantly; notifications are processed off the main request thread.

### Why NOT alternatives

| Alternative | Why Rejected |
|---|---|
| **Direct call in Controller** | Blocks the HTTP response. Violates separation of concerns — Controllers should be thin. |
| **MediatR / in-process event bus** | Adds a library dependency for a single use case. Overkill. The existing `Channel<Func<...>>` queue is already battle-tested here. |
| **Outbox pattern (DB table)** | Correct for at-least-once delivery, but premature for this scope. The background queue + logging is sufficient for now. |
| **Webhooks / external service** | Adds deployment complexity. Not needed yet. |

### Architecture Diagram

```
CheckInParcel (v1 or v2)
    │
    ├── 1. Validate resident unit / locker / duplicate
    ├── 2. Create Parcel + TrackingEvent (committed via UnitOfWork)
    ├── 3. Enqueue notification via INotificationEnqueuer ──────┐
    └── 4. Return ParcelResponseDto                              │
                                                                ▼
                                              NotificationBackgroundService
                                              (picks up from Channel)
                                                    │
                                                    ├── Resolve scoped services
                                                    ├── Find residents for the unit
                                                    ├── Check NotificationPref per resident
                                                    ├── Build message (tracking#, locker)
                                                    └── Call INotificationSender adapters
                                                          ├── EmailSender (SendGrid)
                                                          └── WhatsAppSender (Twilio)
```

---

## 2. File-by-File Implementation

### Layer: Core (`ParcelManagement.Core`)

#### 2.1 New Interface: `INotificationSender`

**Path:** `backend/src/ParcelManagement.Core/Services/INotificationSender.cs`

```csharp
namespace ParcelManagement.Core.Services;

public interface INotificationSender
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendWhatsAppAsync(string toPhone, string message);
}
```

**Purpose:** Abstracts external notification providers. The Core defines *what* to send; Infrastructure defines *how*.

---

#### 2.2 New Service: `ParcelNotificationService`

**Path:** `backend/src/ParcelManagement.Core/Services/ParcelNotificationService.cs`

```csharp
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services;

public interface IParcelNotificationService
{
    Task NotifyResidentsOfParcelArrivalAsync(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName, CancellationToken ct = default);
}

public class ParcelNotificationService : IParcelNotificationService
{
    private readonly IUserResidentUnitRepository _uruRepo;
    private readonly INotificationPrefRepository _npRepo;
    private readonly INotificationSender _notificationSender;
    private readonly IParcelRepository _parcelRepo;

    public ParcelNotificationService(
        IUserResidentUnitRepository uruRepo,
        INotificationPrefRepository npRepo,
        INotificationSender notificationSender,
        IParcelRepository parcelRepo)
    {
        _uruRepo = uruRepo;
        _npRepo = npRepo;
        _notificationSender = notificationSender;
        _parcelRepo = parcelRepo;
    }

    public async Task NotifyResidentsOfParcelArrivalAsync(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName, CancellationToken ct = default)
    {
        // 1. Find all residents linked to this unit
        var uruSpec = new UserResidentUnitByResidentUnitIdSpecification(residentUnitId);
        var unitLinks = await _uruRepo.GetUserResidentUnitsBySpecification(uruSpec);
        var activeResidents = unitLinks
            .Where(link => link.IsActive && link.User != null)
            .Select(link => link.User!)
            .ToList();

        if (activeResidents.Count == 0) return;

        // 2. Build message content
        var lockerPart = string.IsNullOrEmpty(lockerName)
            ? "the parcel room"
            : $"locker {lockerName}";
        var subject = $"📦 Parcel Arrived: {trackingNumber}";
        var body = $"Your parcel {trackingNumber} has arrived and is in {lockerPart}. "
                 + "Please pick it up at your earliest convenience.";

        // 3. For each resident, check preferences and send
        foreach (var resident in activeResidents)
        {
            if (ct.IsCancellationRequested) break;

            var npSpec = new NotificationPrefByUserIdSpecification(resident.Id);
            var pref = await _npRepo.GetNotificationPrefBySpecification(npSpec);

            // If no preferences set, use defaults (IsOnCheckInActive=true, IsEmailActive=true)
            if (pref == null || pref.IsOnCheckInActive)
            {
                // Respect quiet hours if set
                if (!IsWithinQuietHours(pref))
                {
                    var emailActive = pref?.IsEmailActive ?? true;
                    var whatsappActive = pref?.IsWhatsAppActive ?? true;

                    if (emailActive)
                    {
                        await _notificationSender.SendEmailAsync(resident.Email, subject, body);
                    }

                    // WhatsApp requires a phone — User entity currently has no phone field.
                    // This is a known gap. Log and skip for now.
                    if (whatsappActive)
                    {
                        // TODO: Add PhoneNumber to User entity in a future story
                        Console.WriteLine($"[Notification] WhatsApp skipped for {resident.Username}: no phone number on User entity");
                    }
                }
                else
                {
                    Console.WriteLine($"[Notification] Skipped {resident.Username}: within quiet hours");
                }
            }
        }
    }

    private static bool IsWithinQuietHours(NotificationPref? pref)
    {
        if (pref?.QuietHoursFrom == null || pref?.QuietHoursTo == null)
            return false;

        var now = TimeOnly.FromDateTime(DateTime.Now);
        var from = pref.QuietHoursFrom.Value;
        var to = pref.QuietHoursTo.Value;

        if (from <= to)
            return now >= from && now <= to;
        else
            // Spans midnight (e.g., 22:00–06:00)
            return now >= from || now <= to;
    }
}
```

**Key design choices:**
- Uses `UserResidentUnitByResidentUnitIdSpecification` — an existing spec that queries by `ResidentUnitId`. Does NOT currently `.Include("User")`, so the plan documents adding the include string.
- Quiet hours respect midnight-spanning ranges (e.g., 22:00 to 06:00).
- Falls back to defaults (`IsOnCheckInActive=true`) when no `NotificationPref` row exists yet.
- WhatsApp is noted as a gap because `User` has no phone field.

---

#### 2.3 New Enqueuer Interface & Background Service

**Path:** `backend/src/ParcelManagement.Core/BackgroundServices/NotificationBackgroundService.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Core.BackgroundServices;

public interface INotificationEnqueuer
{
    ValueTask EnqueueParcelArrivedNotification(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName);
}

public class NotificationBackgroundService : BackgroundService, INotificationEnqueuer
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationBackgroundService(
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory)
    {
        _taskQueue = taskQueue;
        _scopeFactory = scopeFactory;
    }

    // ── Enqueuer interface ──────────────────────────────
    public ValueTask EnqueueParcelArrivedNotification(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName)
    {
        // Capture locals for the closure (avoid modified closure bugs)
        var pid = parcelId;
        var tn = trackingNumber;
        var ruid = residentUnitId;
        var ln = lockerName;

        return _taskQueue.QueueBackgroundTaskAsync(async ct =>
        {
            using var scope = _scopeFactory.CreateAsyncScope();
            var notificationService = scope.ServiceProvider
                .GetRequiredService<IParcelNotificationService>();
            await notificationService.NotifyResidentsOfParcelArrivalAsync(
                pid, tn, ruid, ln, ct);
        });
    }

    // ── BackgroundService ──────────────────────────────
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log and continue — one failed notification shouldn't kill the loop
                Console.WriteLine($"[NotificationBackgroundService] Error: {ex.Message}");
            }
        }
    }
}
```

**Mirrors `ParcelOverstayBackgroundService` exactly:**
- Same dual-role pattern: implements both `BackgroundService` and `INotificationEnqueuer`
- Same scoped-service resolution via `IServiceScopeFactory`
- Same `IBackgroundTaskQueue` for queueing

---

#### 2.4 Update Existing: `ParcelService.CheckInHelper`

**Path:** `backend/src/ParcelManagement.Core/Services/ParcelService/ParcelService.cs`
**Action:** Inject `INotificationEnqueuer` and fire after parcel creation.

**Current signature (line 11-19):**
```csharp
public partial class ParcelService(
    IParcelRepository parcelRepo,
    IResidentUnitRepository residentUnitRepo,
    IUserRepository userRepo,
    ITrackingEventRepository trackingEventRepo,
    ILockerRepository lockerRepo,
    IUnitOfWork unitOfWork,
    IParcelOverstayEnqueuer parcelOverstayEnqueuer
    ) : IParcelService
```

**New signature:**
```csharp
public partial class ParcelService(
    IParcelRepository parcelRepo,
    IResidentUnitRepository residentUnitRepo,
    IUserRepository userRepo,
    ITrackingEventRepository trackingEventRepo,
    ILockerRepository lockerRepo,
    IUnitOfWork unitOfWork,
    IParcelOverstayEnqueuer parcelOverstayEnqueuer,
    INotificationEnqueuer notificationEnqueuer       // ← NEW
    ) : IParcelService
```

Add private field:
```csharp
private readonly INotificationEnqueuer _notificationEnqueuer = notificationEnqueuer;
```

**Update `CheckInHelper` (line 176-204):** After `await _trackingEventRepo.CreateAsync(newTracking);` add:

```csharp
// Fire-and-forget notification — does not block the response
await _notificationEnqueuer.EnqueueParcelArrivedNotification(
    newParcel.Id, trackingNumber, residentUnitId,
    lockerId.HasValue ? /* resolve locker name */ null : null);
```

**Locker name resolution:** The `CheckInHelper` receives a `Guid? lockerId`. To pass the locker name, add a quick lookup before the enqueue call:

```csharp
string? lockerName = null;
if (lockerId.HasValue)
{
    var locker = await _lockerRepo.GetLockerByIdAsync(lockerId.Value);
    lockerName = locker?.LockerName;
}
await _notificationEnqueuer.EnqueueParcelArrivedNotification(
    newParcel.Id, trackingNumber, residentUnitId, lockerName);
```

> **Note:** `GetLockerByIdAsync` may not exist yet on `ILockerRepository`. If not, add it:
> ```csharp
> // In ILockerRepository:
> Task<Locker?> GetLockerByIdAsync(Guid id);
> ```

---

#### 2.5 Update Existing: `UserResidentUnitByResidentUnitIdSpecification`

**Path:** `backend/src/ParcelManagement.Core/Specifications/UserResidentUnitSpecification.cs`

Currently the spec does NOT include the `User` navigation property. We need `.Include("User")` so `ParcelNotificationService` can access `resident.Email`.

**Change line 38-46:**
```csharp
public class UserResidentUnitByResidentUnitIdSpecification(Guid residentUnitId) :
    ISpecification<UserResidentUnit>
{
    // ... other members unchanged ...

    public List<IncludeExpressionString> IncludeExpressionsString => [
        new IncludeExpressionString("User")    // ← ADD THIS
    ];

    // ...
}
```

---

### Layer: Infrastructure (`ParcelManagement.Infrastructure`)

#### 2.6 Email Sender Adapter

**Path:** `backend/src/ParcelManagement.Infrastructure/Notification/EmailNotificationSender.cs`

```csharp
using ParcelManagement.Core.Services;
using System.Net;
using System.Net.Mail;

namespace ParcelManagement.Infrastructure.Notification;

public class EmailNotificationSender : INotificationSender
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromAddress;

    public EmailNotificationSender(
        string smtpHost, int smtpPort,
        string username, string password, string fromAddress)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _username = username;
        _password = password;
        _fromAddress = fromAddress;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = true
        };
        var mail = new MailMessage(_fromAddress, toEmail, subject, body)
        {
            IsBodyHtml = false
        };
        await client.SendMailAsync(mail);
    }

    public Task SendWhatsAppAsync(string toPhone, string message)
    {
        // Not implemented — awaiting User.PhoneNumber field
        Console.WriteLine($"[EmailNotificationSender] WhatsApp not implemented yet");
        return Task.CompletedTask;
    }
}
```

**Note on SendGrid vs SMTP:** The user story suggests SendGrid. For an MVP, SMTP via `System.Net.Mail` works without new NuGet packages. Upgrading to SendGrid's REST API later is a pure infrastructure swap — zero changes to Core.

---

#### 2.7 (Future) WhatsApp Sender

**Path:** `backend/src/ParcelManagement.Infrastructure/Notification/WhatsAppNotificationSender.cs`

Deferred. Requires:
1. `PhoneNumber` field on `User` entity + migration
2. Twilio NuGet package or WhatsApp Business API client
3. Configuration for Twilio Account SID / Auth Token

---

### Layer: API (`ParcelManagement.Api`)

#### 2.8 Update: `ServiceExtension.cs`

**Path:** `backend/src/ParcelManagement.Api/Extension/ServiceExtension.cs`

Add these registrations inside `AddApplicationServices()`:

```csharp
// ── Notification services ──────────────────────────────

// Adapter: reads SMTP settings from configuration
services.AddSingleton<INotificationSender>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new EmailNotificationSender(
        smtpHost: config["Notification:Email:SmtpHost"] ?? "smtp.sendgrid.net",
        smtpPort: int.Parse(config["Notification:Email:SmtpPort"] ?? "587"),
        username: config["Notification:Email:Username"] ?? "",
        password: config["Notification:Email:Password"] ?? "",
        fromAddress: config["Notification:Email:FromAddress"] ?? "noreply@parcelmgmt.com"
    );
});

services.AddScoped<IParcelNotificationService, ParcelNotificationService>();

// Background service (follows the ParcelOverstayBackgroundService pattern)
services.AddSingleton<NotificationBackgroundService>();
services.AddHostedService(provider =>
    provider.GetRequiredService<NotificationBackgroundService>());
services.AddSingleton<INotificationEnqueuer>(provider =>
    provider.GetRequiredService<NotificationBackgroundService>());
```

---

#### 2.9 Configuration: `appsettings.json`

Add to the settings file:

```json
{
  "Notification": {
    "Email": {
      "SmtpHost": "smtp.sendgrid.net",
      "SmtpPort": 587,
      "Username": "",
      "Password": "",
      "FromAddress": "noreply@parcelmgmt.com"
    },
    "WhatsApp": {
      "AccountSid": "",
      "AuthToken": "",
      "FromNumber": ""
    }
  }
}
```

Credentials should be loaded from environment variables in production (already supported via `DotNetEnv.Env.Load`).

---

### Layer: Core — Supplementary Files

#### 2.10 New Specification (if needed): `UserResidentUnitByUnitIdWithUserSpec`

If modifying the existing `UserResidentUnitByResidentUnitIdSpecification` to always include `User` has unwanted side effects (e.g., it's used in performance-sensitive queries), create a **new** spec instead:

**Path:** `backend/src/ParcelManagement.Core/Specifications/UserResidentUnitSpecification.cs` (append)

```csharp
public class UserResidentUnitByUnitIdWithUserSpec(Guid residentUnitId)
    : ISpecification<UserResidentUnit>
{
    public List<IncludeExpressionString> IncludeExpressionsString => [
        new IncludeExpressionString("User")
    ];

    // ... standard boilerplate ...

    public Expression<Func<UserResidentUnit, bool>> ToExpression()
        => uru => uru.ResidentUnitId == residentUnitId && uru.IsActive;
}
```

This keeps existing queries untouched.

---

## 3. Dependency Injection Wiring (Summary)

```
INotificationSender           → EmailNotificationSender        (Singleton)
IParcelNotificationService    → ParcelNotificationService       (Scoped)
NotificationBackgroundService                                   (Singleton)
INotificationEnqueuer          → NotificationBackgroundService   (Singleton)
NotificationBackgroundService → AddHostedService()              (Hosted)
```

`ParcelService` (already Scoped) receives `INotificationEnqueuer` via constructor injection.

---

## 4. Execution Flow (End-to-End)

```
POST /api/v1/Parcel/checkIn   (or v2)
│
├── ParcelController.CheckInParcel()
│   └── _parcelService.CheckInParcelAsync(...)
│       └── CheckInHelper(trackingNumber, residentUnitId, lockerId, ...)
│           ├── Create Parcel entity
│           ├── Create TrackingEvent (CheckIn)
│           ├── _parcelRepo.AddParcelAsync(newParcel)
│           ├── _trackingEventRepo.CreateAsync(newTracking)
│           └── _notificationEnqueuer.EnqueueParcelArrivedNotification(...)
│               └── QueueBackgroundTaskAsync(workItem)  ← writes to Channel
│
├── Returns CreatedAtAction  ← HTTP response sent HERE, notification still pending
│
│  ... meanwhile, on a background thread ...
│
NotificationBackgroundService.ExecuteAsync()
└── DequeueAsync() → workItem()
    └── Create scope → IParcelNotificationService.NotifyResidentsOfParcelArrivalAsync()
        ├── Find active residents for the unit (UserResidentUnit)
        ├── For each resident:
        │   ├── Get NotificationPref
        │   ├── Skip if IsOnCheckInActive = false
        │   ├── Skip if within quiet hours
        │   └── Send email via INotificationSender.SendEmailAsync()
        └── Log result
```

---

## 5. Testing Strategy

### Unit Tests (`ParcelManagement.Test`)

| Test | What it verifies |
|---|---|
| `ParcelNotificationService_NotifiesActiveResidents` | Mock repos return 2 active residents; verify `SendEmailAsync` called twice |
| `ParcelNotificationService_SkipsOptedOutResident` | `IsOnCheckInActive=false` → no send |
| `ParcelNotificationService_RespectsQuietHours` | Current time inside quiet hours → no send |
| `ParcelNotificationService_HandlesNullNotificationPref` | No pref row → defaults (send) |
| `NotificationBackgroundService_ProcessesQueuedItem` | Enqueue item; verify service resolves and calls notify |
| `ParcelService_CheckIn_EnqueuesNotification` | CheckInParcelAsync → verify `EnqueueParcelArrivedNotification` called with correct args |

### Integration Tests (`ParcelManagement.Test.Integration`)

| Test | What it verifies |
|---|---|
| `CheckIn_TriggersNotificationEnqueue` | Real DB, mock `INotificationEnqueuer`; verify enqueuer called after check-in |
| `NotificationPref_FiltersNotification` | Create real NotificationPref with `IsOnCheckInActive=false`; verify no email |

---

## 6. Acceptance Criteria Mapping

| AC | How It's Met |
|---|---|
| System identifies resident(s) linked to target unit | `ParcelNotificationService` queries `UserResidentUnit` by `ResidentUnitId` |
| Notification triggered based on `NotificationPreference` | Checks `IsOnCheckInActive`, `IsEmailActive`, `IsWhatsAppActive`, quiet hours |
| Contains tracking number + locker name | Message built in `ParcelNotificationService` with `trackingNumber` and `lockerName` |
| Residents can opt-out via `NotificationPrefController` | Existing `PATCH /api/v1/NotificationPref/{id}` already handles this |

---

## 7. File Change Summary

| # | File | Action |
|---|---|---|
| 1 | `Core/Services/INotificationSender.cs` | **New** |
| 2 | `Core/Services/ParcelNotificationService.cs` | **New** |
| 3 | `Core/BackgroundServices/NotificationBackgroundService.cs` | **New** |
| 4 | `Core/Services/ParcelService/ParcelService.cs` | **Modify** (inject + enqueue in CheckInHelper) |
| 5 | `Core/Specifications/UserResidentUnitSpecification.cs` | **Modify** (add Include("User")) |
| 6 | `Infrastructure/Notification/EmailNotificationSender.cs` | **New** |
| 7 | `Api/Extension/ServiceExtension.cs` | **Modify** (register new services) |
| 8 | `Api/appsettings.json` | **Modify** (add Notification config section) |
| 9 | `Core/Repositories/ILockerRepository.cs` | **Maybe Modify** (add GetLockerByIdAsync if missing) |

**Zero database migrations required** — all notification data uses existing `NotificationPref` and `UserResidentUnit` tables.

---

## 8. Estimated Complexity

| Category | Estimate |
|---|---|
| New files | 4 |
| Modified files | 4–5 |
| New NuGet packages | 0 (uses `System.Net.Mail`) |
| DB migrations | 0 |
| Breaking changes | 0 |
| Lines of code | ~300 |
