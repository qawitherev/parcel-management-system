using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface ISystemSettingService
    {
        Task<IReadOnlyList<SystemSetting>> GetSystemSettings(FilterPaginationRequest<SystemSettingSortableColumn> filter);
        Task<SystemSetting> CreateSystemSettingAsync(SystemSettingType type, string? value);
        Task<SystemSetting> GetSystemSettingByIdAsync(Guid id);
        Task<SystemSetting?> GetSystemSettingByNameAsync(SystemSettingType type);
        Task<SystemSetting> UpdateSystemSettingAsync(Guid id, string? value);
        Task DeleteSystemSettingAsync(Guid id);
    }

    public class SystemSettingService : ISystemSettingService
    {
        private readonly ISystemSettingRepository _settingRepo;

        public SystemSettingService(ISystemSettingRepository settingRepo)
        {
            _settingRepo = settingRepo;
        }

        public async Task<SystemSetting> CreateSystemSettingAsync(SystemSettingType type, string? value)
        {
            var spec = new SystemSettingByTypeSpecification(type);
            var existing = await _settingRepo.GetSystemSettingBySpecification(spec);

            var setting = new SystemSetting
            {
                Id = Guid.NewGuid(),
                Value = value
            };

            await _settingRepo.CreateSystemSettingAsync(setting);
            return setting;
        }

        public async Task<SystemSetting> GetSystemSettingByIdAsync(Guid id)
        {
            return await _settingRepo.GetSystemSettingByIdAsync(id) 
                ?? throw new KeyNotFoundException($"System setting not found");
        }

        public async Task<SystemSetting?> GetSystemSettingByNameAsync(SystemSettingType type)
        {
            var spec = new SystemSettingByTypeSpecification(type);
            return await _settingRepo.GetSystemSettingBySpecification(spec);
        }

        public async Task<SystemSetting> UpdateSystemSettingAsync(Guid id, string? value)
        {
            var existing = await _settingRepo.GetSystemSettingByIdAsync(id) ??
                throw new KeyNotFoundException("System setting not found");

            existing.Value = value;
            await _settingRepo.UpdateSystemSettingAsync(existing);
            return existing;
        }

        public async Task DeleteSystemSettingAsync(Guid id)
        {
            await _settingRepo.DeleteSystemSettingAsync(id);
        }

        public async Task<IReadOnlyList<SystemSetting>> GetSystemSettings(FilterPaginationRequest<SystemSettingSortableColumn> filter)
        {
            var spec = new SystemSettingViewSpecification(filter);
            return await _settingRepo.GetSystemSettingsBySpecification(spec);
        }
    }
}