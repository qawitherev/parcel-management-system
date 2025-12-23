using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface INotificationPrefRepository
    {
        // crud 
        Task<NotificationPref> CreateNotificationPrefAsync(NotificationPref np);

        Task<NotificationPref?> GetNotificationPrefAsync(Guid id);

        Task UpdateNotificationPrefAsync(NotificationPref np);

        Task DeleteNotificationPrefAsync(Guid id);

        Task<IReadOnlyList<NotificationPref>> GetNotificationPrefsBySpecification(ISpecification<NotificationPref> specification);

        Task<NotificationPref?> GetNotificationPrefBySpecification(ISpecification<NotificationPref> specification);

        Task<int> GetNotificationPrefCountBySpecification(ISpecification<NotificationPref> specification);
    }
}