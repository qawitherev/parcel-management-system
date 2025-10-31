using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IResidentUnitService
    {
        Task<ResidentUnit> GetResidentUnitById(Guid residentUnitId);

        Task<ResidentUnit> CreateResidentUnitAsync(string unitName, Guid userId);

        Task UpdateResidentUnitAsync(Guid id, string unitName, Guid performedBy);

        Task<(IReadOnlyList<ResidentUnit>, int count)> GetResidentUnitsForViewAsync(
            string? unitName,
            ResidentUnitSortableColumn? column,
            int? page,
            int? take,
            bool isAsc = true
        );

        Task<ResidentUnit> GetResidentsForUnitAsync(Guid residentUnitId);
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

        public async Task UpdateResidentUnitAsync(Guid id, string unitName, Guid performedBy)
        {
            var existing = await _residentUnitRepo.GetResidentUnitByIdAsync(id) ??
                throw new KeyNotFoundException($"Unit {unitName} not found");
            var spec = new ResidentUnitByUnitNameSpecification(unitName);
            var existingName = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(spec);
            if (existingName != null && existingName.Id != id)
            {
                throw new InvalidOperationException($"{unitName} has been taken. Try another name");
            }
            existing.UnitName = unitName;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = performedBy;
            await _residentUnitRepo.UpdateResidenUnitAsync(existing);
        }

        public async Task<(IReadOnlyList<ResidentUnit>, int count)> GetResidentUnitsForViewAsync(
            string? unitName,
            ResidentUnitSortableColumn? column,
            int? page,
            int? take,
            bool isAsc = true
        )
        {
            var spec = new ResidentUnitViewSpecification(unitName, column, page, take, isAsc);
            var residentUnits = await _residentUnitRepo.GetResidentUnitsBySpecificationAsync(spec);
            var count = await _residentUnitRepo.GetResidentUnitCountBySpecification(spec);
            return (residentUnits, count);
        }

        public async Task<ResidentUnit> GetResidentsForUnitAsync(Guid residentUnitId)
        {
            var specification = new ResidentUnitResidentsSpecification(residentUnitId);
            var residentUnits = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(specification) ??
                throw new KeyNotFoundException("Resident unit does not exist");
            return residentUnits;
        }
    }
}