using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Core.Services
{
    public interface ILockerService
    {
        Task<Locker> CreateLockerAsync(string lockerName, Guid performedByUser);
        Task<Locker> GetLockerByIdAsync(Guid id);
        Task<(IReadOnlyList<Locker>, int count)> GetLockersAsync(
            FilterPaginationRequest<LockerSortableColumn> filterRequest
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

        public async Task<Locker> GetLockerByIdAsync(Guid id)
        {
            return await _lockerRepo.GetLockerByIdAsync(id) ??
                throw new KeyNotFoundException($"Locker not found");
        }

        public async Task<(IReadOnlyList<Locker>, int count)> GetLockersAsync(
            FilterPaginationRequest<LockerSortableColumn> filterRequest)
        {
            var spec = new AllLockersSpecification(filterRequest);
            var lockers = await _lockerRepo.GetLockersBySpecificationAsync(spec);
            var count = await _lockerRepo.GetLockerCountBySpecification(spec);
            return (lockers, count);
        }

        public async Task UpdateLockerAsync(Locker locker)
        {
            await _lockerRepo.UpdateLockerAsync(locker);
        }
    }
}