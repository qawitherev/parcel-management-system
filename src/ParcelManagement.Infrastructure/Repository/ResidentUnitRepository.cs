using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class ResidentUnitRepository(ApplicationDbContext dbContext) : IResidentUnitRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<ResidentUnit> CreateResidentUnitAsync(ResidentUnit residentUnit)
        {
            await _dbContext.ResidentUnits.AddAsync(residentUnit);
            await _dbContext.SaveChangesAsync();
            return residentUnit;
        }

        public async Task DeleteResidentUnitAsync(Guid id)
        {
            var toBeRemoved = await _dbContext.ResidentUnits.FindAsync(id) ??
                throw new NullReferenceException($"Resident unit with id {id} does not exist");
            _dbContext.ResidentUnits.Remove(toBeRemoved);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ResidentUnit?> GetOneResidentUnitBySpecificationAsync(ISpecification<ResidentUnit> specification)
        {
            return await _dbContext.ResidentUnits.Where(specification.ToExpression()).FirstOrDefaultAsync();
        }

        public async Task<ResidentUnit?> GetResidentUnitByIdAsync(Guid id)
        {
            return await _dbContext.ResidentUnits.FindAsync(id);
        }

        // TODO
        // to add pagination to all get repo implementation
        public async Task<IReadOnlyList<ResidentUnit?>> GetResidentUnitsAsync()
        {
            return await _dbContext.ResidentUnits.ToListAsync();
        }

        public async Task<IReadOnlyList<ResidentUnit?>> GetResidentUnitsBySpecificationAsync(ISpecification<ResidentUnit> specification)
        {
            return await _dbContext.ResidentUnits.Where(specification.ToExpression()).ToListAsync();
        }

        public async Task UpdateResidenUnitAsync(ResidentUnit residentUnit)
        {
            _dbContext.Entry(residentUnit).CurrentValues.SetValues(residentUnit);
            await _dbContext.SaveChangesAsync();
        }
    }
}