using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SystemSettingRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SystemSetting> CreateSystemSettingAsync(SystemSetting setting)
        {
            await CreateAsync(setting);
            return setting;
        }

        public async Task<SystemSetting?> GetSystemSettingByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<SystemSetting?> GetSystemSettingByNameAsync(string name)
        {
            return await _dbContext.Set<SystemSetting>()
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task UpdateSystemSettingAsync(SystemSetting setting)
        {
            await UpdateAsync(setting);
        }

        public async Task DeleteSystemSettingAsync(Guid id)
        {
            await DeleteAsync(id);
        }

        public async Task<IReadOnlyList<SystemSetting>> GetSystemSettingsBySpecification(ISpecification<SystemSetting> specification)
        {
            return await GetBySpecificationAsync(specification);
        }

        public async Task<SystemSetting?> GetSystemSettingBySpecification(ISpecification<SystemSetting> specification)
        {
            return await GetOneBySpecificationAsync(specification);
        }

        public async Task<int> GetSystemSettingCountBySpecification(ISpecification<SystemSetting> specification)
        {
            return await GetCountBySpecificationAsync(specification);
        }
    }
}