using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class UserResidentUnitRepository(ApplicationDbContext dbContext) : IUserResidentUnitRepository
    {
        private readonly ApplicationDbContext _dbContext = dbContext;

        public async Task<UserResidentUnit> CreateUserResidentUnitAsync(UserResidentUnit userResidentUnit)
        {
            await _dbContext.UserResidentUnits.AddAsync(userResidentUnit);
            await _dbContext.SaveChangesAsync();
            return userResidentUnit;
        }

        // dont think we'll ever use this one because 
        // for bridge table, use softDelete
        public async Task DeleteUserResidentUnit(Guid Id)
        {
            var uru = await _dbContext.UserResidentUnits.FindAsync(Id);
            _dbContext.UserResidentUnits.Remove(uru!);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserResidentUnit?> GetOneResidentUnitBySpecification(ISpecification<UserResidentUnit> specification)
        {
            return await _dbContext.UserResidentUnits.Where(specification.ToExpression()).FirstOrDefaultAsync();

        }

        public async Task<UserResidentUnit?> GetResidentUnitByCombination(Guid userId, Guid residentUnitId)
        {
            return await _dbContext.UserResidentUnits.Where(
                ent => ent.UserId == userId &&
                ent.ResidentUnitId == residentUnitId
            ).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ResidentUnit?>> GetResidentUnitsByUser(Guid userId)
        {
            return await _dbContext.UserResidentUnits
                .Include(uru => uru.ResidentUnit)
                .Where(uru => uru.UserId == userId)
                .Select(uru => uru.ResidentUnit)
                .ToListAsync();
        }

        public async Task<UserResidentUnit?> GetUserResidentUnitByIdAsync(Guid id)
        {
            var res = await _dbContext.UserResidentUnits.FindAsync(id);
            return res;
        }

        public async Task<IReadOnlyCollection<UserResidentUnit?>> GetUserResidentUnitsBySpecification(ISpecification<UserResidentUnit> specification)
        {
            return await _dbContext.UserResidentUnits.Where(specification.ToExpression()).ToListAsync();

        }

        public async Task<IReadOnlyCollection<User?>> GetUsersByResidentUnit(Guid residentUnitId)
        {
            return await _dbContext.UserResidentUnits
                .Include(uru => uru.User)
                .Where(uru => uru.ResidentUnitId == residentUnitId)
                .Select(uru => uru.User)
                .ToListAsync();
        }

        public async Task UpdateUserResidentUnit(UserResidentUnit userResidentUnit)
        {
            var existing = await _dbContext.UserResidentUnits.FindAsync(userResidentUnit.Id) ??
                throw new NullReferenceException($"UserResidentUnit with id {userResidentUnit.Id} does not exist");
            _dbContext.Entry(existing).CurrentValues.SetValues(userResidentUnit);
            await _dbContext.SaveChangesAsync();
        }
    }
}