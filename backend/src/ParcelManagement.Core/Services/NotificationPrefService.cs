using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.NotificationPref;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface INotificationPrefService
    {
        Task<NotificationPref> CreateNotificationPrefAsync(NotificationPrefCreateRequest np);

        Task<NotificationPref?> GetNotificationPrefByIdAsync(Guid id);

        Task UpdateNotificationPrefs (NotificationPrefUpdateRequest np, Guid updatingUserId);

        Task<NotificationPref?> GetNotificationPrefByUserId(Guid userId);
    }

    public class NotificationPrefService : INotificationPrefService
    {
        private readonly INotificationPrefRepository _npRepo;
        private readonly IUserRepository _userRepo;

        public NotificationPrefService(INotificationPrefRepository npRepo, IUserRepository userRepo)
        {
            _npRepo = npRepo;
            _userRepo = userRepo;
        }

        public async Task<NotificationPref> CreateNotificationPrefAsync(NotificationPrefCreateRequest np)
        {
            var specification = new NotificationPrefByUserIdSpecification(np.UserId);
            var existing = await _npRepo.GetNotificationPrefBySpecification(specification);
            if (existing != null)
            {
                throw new InvalidOperationException($"User already has notification preferences");
            }
            var notificationPref = new NotificationPref
            {
                Id = Guid.NewGuid(), 
                UserId = np.UserId, 
                IsEmailActive = np.IsEmailActive, 
                IsWhatsAppActive = np.IsWhatsAppActive, 
                IsOnCheckInActive = np.IsOnCheckInActive, 
                IsOnClaimActive = np.IsOnClaimActive, 
                IsOverdueActive = np.IsOverdueActive, 
                QuietHoursFrom = np.QuietHoursFrom, 
                QuietHoursTo = np.QuietHoursTo,
                CreatedBy = np.CreatingUserId, 
                CreatedOn = DateTimeOffset.UtcNow, 
            };
            var newNp = await _npRepo.CreateNotificationPrefAsync(notificationPref);
            return newNp;
        }

        public async Task<NotificationPref?> GetNotificationPrefByIdAsync(Guid id)
        {
            var np = await _npRepo.GetNotificationPrefByIdAsync(id) ?? 
                throw new KeyNotFoundException($"Notification preferences not found");
            return np;
        }

        public async Task<NotificationPref?> GetNotificationPrefByUserId(Guid userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId) ?? 
                throw new KeyNotFoundException($"User not found");
            var specification = new NotificationPrefByUserIdSpecification(user.Id);
            var notiPrefs = await _npRepo.GetNotificationPrefBySpecification(specification);
            return notiPrefs;
        }

        public async Task UpdateNotificationPrefs(NotificationPrefUpdateRequest np, Guid updatingUserId)
        {
            var existing = await _npRepo.GetNotificationPrefByIdAsync(np.NotificationPrefId) ?? 
                throw new KeyNotFoundException($"Notification preferences not found");
            if (updatingUserId != existing.UserId)
            {
                throw new InvalidOperationException("Notification preference cannot be updated");
            }
            existing.IsEmailActive = np.IsEmailActive ?? existing.IsEmailActive;
            existing.IsWhatsAppActive = np.IsWhatsAppActive ?? existing.IsWhatsAppActive;
            existing.IsOnCheckInActive = np.IsOnCheckInActive ?? existing.IsOnCheckInActive;
            existing.IsOnClaimActive = np.IsOnClaimActive ?? existing.IsOnClaimActive;
            existing.IsOverdueActive = np.IsOverdueActive ?? existing.IsOverdueActive;
            existing.QuietHoursFrom = np.QuietHoursFrom;
            existing.QuietHoursTo = np.QuietHoursTo;
            existing.UpdatedBy = np.UpdatingUserId;
            existing.UpdatedOn = DateTimeOffset.UtcNow;

            await _npRepo.UpdateNotificationPrefAsync(existing);
        }
    }
}