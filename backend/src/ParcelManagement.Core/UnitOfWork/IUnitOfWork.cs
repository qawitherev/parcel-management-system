using Microsoft.EntityFrameworkCore.Storage;

namespace ParcelManagement.Core.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<IDisposableTransaction> BeginTransaction();
        Task SaveChangesAsync(); 
    }

    public interface IDisposableTransaction: IDisposable, IAsyncDisposable
    {
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}