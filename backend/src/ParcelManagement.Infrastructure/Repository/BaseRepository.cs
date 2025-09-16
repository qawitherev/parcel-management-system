using System.Linq;
using Microsoft.EntityFrameworkCore;
using ParcelManagement.Core.Repositories;
using ParcelManagement.Core.Specifications;
using ParcelManagement.Infrastructure.Database;

namespace ParcelManagement.Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        public BaseRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IReadOnlyList<T>> GetBySpecificationAsync(ISpecification<T> specification)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            query = IncludeExpressionHelper.IncludeExpressionsHelper(
                specification,
                query
            );

            // we probably have to check for null for safety here 
            query = query.Where(specification.ToExpression());
            if (specification.Page.HasValue && specification.Take.HasValue)
            {
                var skip = (specification.Page.Value - 1) * specification.Take.Value;
                query = query.Skip(skip);
                query = query.Take(specification.Take.Value);
            }

            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            if (specification.OrderByDesc != null)
            {
                query = query.OrderByDescending(specification.OrderByDesc);
            }


            return await query.ToListAsync();
        }

        public async Task<T?> GetOneBySpecificationAsync(ISpecification<T> specification)
        {
            IQueryable<T> query = _dbContext.Set<T>();

            query = IncludeExpressionHelper.IncludeExpressionsHelper(
                specification,
                query
            );

            query = query.Where(specification.ToExpression());

            return await query.FirstOrDefaultAsync();
        }

        public async Task<T> CreateAsync(T obj)
        {
            await _dbContext.Set<T>().AddAsync(obj);
            await _dbContext.SaveChangesAsync();
            return obj;
        }

        public async Task<T?> FindByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<ICollection<T>>? GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<int> GetCountBySpecificationAsync(ISpecification<T> specification)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            query = IncludeExpressionHelper.IncludeExpressionsHelper(specification, query);
            query = query.Where(specification.ToExpression());

            return await query.CountAsync();
        }

        // we dont do update here because we have entity that has composite key
        // instad of ID 
    }

    // helper function so that we dont repeat ourself
    public class IncludeExpressionHelper
    {
        public static IQueryable<T> IncludeExpressionsHelper<T>(
            ISpecification<T> specification,
            IQueryable<T> theQuery
            ) where T : class
        {
            foreach (var include in specification.IncludeExpressionString)
            {
                // left join
                theQuery = EntityFrameworkQueryableExtensions.Include(theQuery, include.Path);

            }

            return theQuery;
        }
    }
}