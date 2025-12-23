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

        Task UpdateNotificationPrefs (NotificationPref np);

        Task<NotificationPref?> GetNotificationPrefByUserId(Guid userId);
    }

    public class NotificationService : INotificationPrefService
    {
        private readonly INotificationPrefRepository _npRepo;
        private readonly IUserRepository _userRepo;

        public NotificationService(INotificationPrefRepository npRepo, IUserRepository userRepo)
        {
            _npRepo = npRepo;
            _userRepo = userRepo;
        }

        public async Task<NotificationPref> CreateNotificationPrefAsync(NotificationPrefCreateRequest np)
        {
            var notificationPref = new NotificationPref
            {
                Id = Guid.NewGuid(), 
                UserId = np.UserId, 
                IsEmailActive = np.IsEmailActive, 
                IsWhatsAppActive = np.IsWhatsAppActive, 
                IsOnCheckInActive = np.IsOnCheckInActive, 
                IsOnClaimActive = np.IsOnClaimActive, 
                IsOverdueActive = np.IsOverdueActive, 
                QuiteHoursFrom = np.QuiteHoursFrom, 
                QuiteHoursTo = np.QuiteHoursTo, 
                CreatedBy = np.UserId, 
                CreatedOn = DateTimeOffset.UtcNow, 
            };
            var newNp = await _npRepo.CreateNotificationPrefAsync(notificationPref);
            return newNp;
        }

        public async Task<NotificationPref?> GetNotificationPrefByIdAsync(Guid id)
        {
            var np = await _npRepo.GetNotificationPrefAsync(id);
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

        public async Task UpdateNotificationPrefs(NotificationPref np)
        {
            await _npRepo.UpdateNotificationPrefAsync(np);
        }
    }
}