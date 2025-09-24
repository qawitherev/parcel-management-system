using Microsoft.AspNetCore.Mvc.Filters;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Api.Filter
{
    public class TransactionFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _dbContext;

        public TransactionFilter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // var method = context.HttpContext.Request.Method;
            // if (method == HttpMethod.Post.ToString() ||
            //     method == 
            // )
        }
    }
}