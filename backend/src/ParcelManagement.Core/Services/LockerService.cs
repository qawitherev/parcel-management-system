using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface ILockerService
    {
        Task<Locker> CreateLockerAsync(string lockerName, Guid performedByUser);
        Task<Locker?> GetLockerByIdAsync(Guid id);
        Task<IReadOnlyList<Locker>> GetLockersAsync(
            string? lockerName,
            LockerSortableColumn? column,
            int? page,
            int? take,
            bool isAsc = true
        );
        Task UpdateLockerAsync(Locker locker);
    }

    public class LockerService : ILockerService
    {
        private readonly ILockerRepository _lockerRepo;

        public LockerService(
            ILockerRepository lockerRepo
        )
        {
            _lockerRepo = lockerRepo;
        }

        public async Task<Locker> CreateLockerAsync(string lockerName, Guid performedByUser)
        {
            var spec = new LockerByLockerNameSpecification(lockerName);
            var existingLocker = await _lockerRepo.GetOneLockerBySpecification(spec);
            if (existingLocker != null)
            {
                throw new InvalidOperationException($"Locker {lockerName} has been registered");
            }
            var newLocker = new Locker
            {
                Id = Guid.NewGuid(),
                LockerName = lockerName,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = performedByUser,
                IsActive = true
            };
            await _lockerRepo.CreateLockerAsync(newLocker);
            return newLocker;
        }

        public Task<Locker?> GetLockerByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Locker>> GetLockersAsync(string? lockerName, LockerSortableColumn? column, int? page, int? take, bool isAsc = true)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLockerAsync(Locker locker)
        {
            throw new NotImplementedException();
        }
    }
}