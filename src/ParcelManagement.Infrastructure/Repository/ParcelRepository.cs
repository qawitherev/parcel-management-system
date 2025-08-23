// author: Abdul Qawi ft. Gemini 2.5 (totally not vibe coded)

using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class ParcelRepository : IParcelRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ParcelRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Parcel> AddParcelAsync(Parcel parcel)
        {
            await _dbContext.Parcels.AddAsync(parcel);
            await _dbContext.SaveChangesAsync();
            return parcel;
        }

        public async Task DeleteParcelAsync(Guid id)
        {
            var existingParcel = await _dbContext.Parcels.FindAsync(id) ?? throw new ArgumentException($"Parcel with id ${id} does not exist");
            _dbContext.Parcels.Remove(existingParcel);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<IReadOnlyList<Parcel?>> GetAllParcelsAsync()
        {
            return await _dbContext.Parcels.ToListAsync();
        }

        public async Task<Parcel?> GetParcelByIdAsync(Guid id)
        {
            return await _dbContext.Parcels.FindAsync(id);
        }

        // get the parcel from memory (EF core is tracking it)
        // if not found, throw error, just a simple one, later we do a more advanced error handling class
        // compare parcel in memory and passed param
        // only update whats changed, voila! EFFICIENCY
        public async Task UpdateParcelAsync(Parcel parcel)
        {
            var existingParcel = await _dbContext.Parcels.FindAsync(parcel.Id) ?? throw new ArgumentException($"Parcel with id {parcel.Id} does not exist");
            _dbContext.Entry(existingParcel).CurrentValues.SetValues(parcel);
            await _dbContext.SaveChangesAsync();
        }

        // SPECIFICATION PATTERN 
        // this is good stuff 
        public async Task<IReadOnlyList<Parcel?>> GetParcelsBySpecificationAsync(ISpecification<Parcel> specification)
        {
            return await _dbContext.Parcels.Where(specification.ToExpression()).ToListAsync();
        }

        public async Task<Parcel?> GetOneParcelBySpecificationAsync(ISpecification<Parcel> specification)
        {
            return await _dbContext.Parcels.Where(specification.ToExpression()).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<Parcel>> GetParcelsByUserAsync(Guid userId)
        {
            var parcels = await _dbContext.Parcels
                            .Include(p => p.ResidentUnit)
                            .ThenInclude(ru => ru!.UserResidentUnits)
                            .ThenInclude(uru => uru.User)
                            .Where(
                                p => p.ResidentUnit != null &&
                                    p.ResidentUnit.UserResidentUnits.Any(uru => uru.UserId == userId)
                            )
                            .ToListAsync();

            return parcels;
        }
    }
}