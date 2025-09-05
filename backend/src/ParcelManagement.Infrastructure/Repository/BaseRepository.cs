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

            query = query.Where(specification.ToExpression());
            if (specification.Skip.HasValue) query = query.Skip(specification.Skip.Value);
            if (specification.Take.HasValue) query = query.Take(specification.Take.Value);

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
            foreach (var include in specification.IncludeExpressions)
            {
                var queryWithInclude = theQuery.Include(include.Path);

                foreach (var thenInclude in include.ThenIncludePaths)
                {
                    queryWithInclude = queryWithInclude.ThenInclude(thenInclude);
                }
            }

            return theQuery;
        }
    }
}