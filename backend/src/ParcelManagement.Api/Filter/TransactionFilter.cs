using Microsoft.AspNetCore.Mvc.Filters;
using ParcelManagement.Core.UnitOfWork;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Api.Filter
{
    public class TransactionFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        public TransactionFilter(
            IUnitOfWork unitOfWork
            )
        {
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
                Console.WriteLine($"ACTION-FILTER: inside transaction filter");
                await next();
            }
        }
    }
}