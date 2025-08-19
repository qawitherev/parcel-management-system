using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface IResidentUnitService
    {
        Task<ResidentUnit> CreateResidentUnitAsync(ResidentUnit residentUnit);

        Task UpdateResidentUnitAsync(ResidentUnit residentUnit);
    }

    public class ResidentUnitService(IResidentUnitRepository residentUnitRepository) : IResidentUnitService
    {
        private readonly IResidentUnitRepository _residentUnitRepo = residentUnitRepository;

        public async Task<ResidentUnit> CreateResidentUnitAsync(ResidentUnit residentUnit)
        {
            var specByUnitName = new ResidentUnitByUnitNameSpecification(residentUnit.UnitName);
            var existing = await _residentUnitRepo.GetOneResidentUnitBySpecificationAsync(specByUnitName);
            if (existing != null) { throw new InvalidOperationException($"Resident Unit {residentUnit.UnitName} has already exist"); }
            return await _residentUnitRepo.CreateResidentUnitAsync(residentUnit);
        }

        public async Task UpdateResidentUnitAsync(ResidentUnit residentUnit)
        {
            var existing = await _residentUnitRepo.GetResidentUnitByIdAsync(residentUnit.Id) ??
                throw new NullReferenceException($"Resident unit {residentUnit.UnitName} does not exist");
            await _residentUnitRepo.UpdateResidenUnitAsync(residentUnit);
        }
    }
}