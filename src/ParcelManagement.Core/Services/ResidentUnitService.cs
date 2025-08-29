using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IResidentUnitService
    {
        Task<ResidentUnit> GetResidentUnitById(Guid residentUnitId);

        Task<ResidentUnit> CreateResidentUnitAsync(string unitName, Guid userId);

        Task UpdateResidentUnitAsync(ResidentUnit residentUnit);
    }

    public class ResidentUnitService(IResidentUnitRepository residentUnitRepository) : IResidentUnitService
    {
        private readonly IResidentUnitRepository _residentUnitRepo = residentUnitRepository;

        public async Task<ResidentUnit> GetResidentUnitById(Guid residentUnitId)
        {
            return await _residentUnitRepo.GetResidentUnitByIdAsync(residentUnitId) ??
                throw new KeyNotFoundException($"Unit not found");
        }
        public async Task<ResidentUnit> CreateResidentUnitAsync(string unitName, Guid userId)
        {
            var specByUnitName = new ResidentUnitByUnitNameSpecification(unitName);
            var existing = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(specByUnitName);
            if (existing != null) { throw new InvalidOperationException($"Resident Unit {unitName} has already exist"); }
            return await _residentUnitRepo.CreateResidentUnitAsync(new ResidentUnit
            {
                Id = Guid.NewGuid(),
                UnitName = unitName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = userId
            });
        }

        public async Task UpdateResidentUnitAsync(ResidentUnit residentUnit)
        {
            var existing = await _residentUnitRepo.GetResidentUnitByIdAsync(residentUnit.Id) ??
                throw new NullReferenceException($"Resident unit {residentUnit.UnitName} does not exist");
            await _residentUnitRepo.UpdateResidenUnitAsync(residentUnit);
        }
    }
}