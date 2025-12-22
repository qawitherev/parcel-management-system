using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class NotificationPrefRepository : BaseRepository<NotificationPref>, INotificationPrefRepository
    {
        public NotificationPrefRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public Task<NotificationPref> CreateNotificationPrefAsync(NotificationPref np)
        {
            throw new NotImplementedException();
        }

        public Task DeleteNotificationPrefAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<NotificationPref> GetNotificationPrefAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<NotificationPref> GetNotificationPrefBySpecification(ISpecification<NotificationPref> specification)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetNotificationPrefCountBySpecification(ISpecification<NotificationPref> specification)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<NotificationPref>> GetNotificationPrefsBySpecification(ISpecification<NotificationPref> specification)
        {
            throw new NotImplementedException();
        }

        public Task UpdateNotificationPrefAsync(NotificationPref np)
        {
            throw new NotImplementedException();
        }
    }
}