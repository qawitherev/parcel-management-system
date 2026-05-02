using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Repositories
{
    public interface ISystemSettingRepository
    {
        // basic CRUD
        Task<SystemSetting> CreateSystemSettingAsync(SystemSetting setting);

        Task<SystemSetting?> GetSystemSettingByIdAsync(Guid id);

        /// <summary>
        /// Convenience lookup when callers only know the name of the setting.
        /// </summary>
        Task<SystemSetting?> GetSystemSettingByNameAsync(SystemSettingType type);

        Task UpdateSystemSettingAsync(SystemSetting setting);

        Task DeleteSystemSettingAsync(Guid id);

        // specification helpers (mirrors other repository patterns)
        Task<IReadOnlyList<SystemSetting>> GetSystemSettingsBySpecification(ISpecification<SystemSetting> specification);

        Task<SystemSetting?> GetSystemSettingBySpecification(ISpecification<SystemSetting> specification);

        Task<int> GetSystemSettingCountBySpecification(ISpecification<SystemSetting> specification);
    }
}