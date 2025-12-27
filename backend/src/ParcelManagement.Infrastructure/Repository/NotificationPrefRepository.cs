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
            // nothing yet 
        }

        public async Task<NotificationPref> CreateNotificationPrefAsync(NotificationPref np)
        {
            await CreateAsync(np);
            return np;
        }

        public Task DeleteNotificationPrefAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<NotificationPref?> GetNotificationPrefByIdAsync(Guid id)
        {
            return await FindByIdAsync(id);

        }

        public async Task<NotificationPref?> GetNotificationPrefBySpecification(ISpecification<NotificationPref> specification)
        {
            return await GetOneBySpecificationAsync(specification);
        }

        public async Task<int> GetNotificationPrefCountBySpecification(ISpecification<NotificationPref> specification)
        {
            return await GetCountBySpecificationAsync(specification);
        }

        public async Task<IReadOnlyList<NotificationPref>> GetNotificationPrefsBySpecification(ISpecification<NotificationPref> specification)
        {
            return await GetBySpecificationAsync(specification);
        }

        public async Task UpdateNotificationPrefAsync(NotificationPref np)
        {
            await UpdateAsync(np);
        }
    }
}