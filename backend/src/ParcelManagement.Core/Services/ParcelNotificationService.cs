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

    public ParcelNotificationService(
        IUserResidentUnitRepository uruRepo,
        INotificationPrefRepository npRepo,
        INotificationSender notificationSender)
    {
        _uruRepo = uruRepo;
        _npRepo = npRepo;
        _notificationSender = notificationSender;
    }

    public async Task NotifyResidentsOfParcelArrivalAsync(
        Guid parcelId, string trackingNumber, Guid residentUnitId,
        string? lockerName, CancellationToken ct = default)
    {
        // 1. Find all active residents linked to this unit
        var uruSpec = new UserResidentUnitByResidentUnitIdSpecification(residentUnitId);
        var unitLinks = await _uruRepo.GetUserResidentUnitsBySpecification(uruSpec);
        var activeResidents = unitLinks
            .Where(link => link.IsActive && link.User != null)
            .Select(link => link.User!)
            .ToList();

        if (activeResidents.Count == 0) return;

        // 2. Build message
        var lockerPart = string.IsNullOrEmpty(lockerName)
            ? "the parcel room"
            : $"locker {lockerName}";
        var subject = $"Parcel Arrived: {trackingNumber}";
        var body = $"Your parcel {trackingNumber} has arrived and is in {lockerPart}. "
                 + "Please pick it up at your earliest convenience.";

        // 3. For each resident, check preferences and send
        foreach (var resident in activeResidents)
        {
            if (ct.IsCancellationRequested) break;

            var npSpec = new NotificationPrefByUserIdSpecification(resident.Id);
            var pref = await _npRepo.GetNotificationPrefBySpecification(npSpec);

            // Default: IsOnCheckInActive = true if no preferences set
            if (pref == null || pref.IsOnCheckInActive)
            {
                if (!IsWithinQuietHours(pref))
                {
                    var emailActive = pref?.IsEmailActive ?? true;

                    if (emailActive)
                    {
                        await _notificationSender.SendEmailAsync(resident.Email, subject, body);
                    }

                    // WhatsApp requires a phone number — User entity has no Phone field yet.
                    // Log and skip for now. TODO: add PhoneNumber to User entity.
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

        // Handle midnight-spanning ranges (e.g., 22:00–06:00)
        if (from <= to)
            return now >= from && now <= to;
        else
            return now >= from || now <= to;
    }
}
