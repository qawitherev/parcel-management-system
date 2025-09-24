using Microsoft.AspNetCore.Mvc.Filters;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.UnitOfWork;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Api.Filter
{
    public class TransactionFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        public TransactionFilter(
            ApplicationDbContext dbContext,
            IUnitOfWork unitOfWork
            )
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            if (method == HttpMethod.Post.ToString() ||
            method == HttpMethod.Put.ToString() ||
            method == HttpMethod.Patch.ToString() ||
            method == HttpMethod.Delete.ToString()
            )
            {
                await using var transaction = await _unitOfWork.BeginTransaction();
                try
                {
                    var result = await next();
                    if (result.Exception != null)
                    {
                        await transaction.RollbackTransactionAsync();
                    }
                    else
                    {
                        await transaction.CommitTransactionAsync();
                    }

                }
                catch
                {
                    await transaction.RollbackTransactionAsync();
                    throw;
                }

            }
            else
            {
                await next();
            }
        }
    }
}