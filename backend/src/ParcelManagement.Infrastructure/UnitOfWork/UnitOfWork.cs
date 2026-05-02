using Microsoft.EntityFrameworkCore.Storage;
using ParcelManagement.Core.UnitOfWork;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IDisposableTransaction> BeginTransaction()
        {
            return new DisposableTransaction(await _dbContext.Database.BeginTransactionAsync());
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        private class DisposableTransaction : IDisposableTransaction
        {
            private readonly IDbContextTransaction _transaction;

            public DisposableTransaction(IDbContextTransaction transaction)
            {
                _transaction = transaction;
            }

            public async Task CommitTransactionAsync()
            {
                await _transaction.CommitAsync();
            }

            public void Dispose()
            {
                _transaction.Dispose();
            }

            public async ValueTask DisposeAsync()
            {
                await _transaction.DisposeAsync();
            }

            public async Task RollbackTransactionAsync()
            {
                await _transaction.RollbackAsync();
            }
        }
    }
    
}