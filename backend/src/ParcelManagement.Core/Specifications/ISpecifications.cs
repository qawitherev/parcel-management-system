using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace ParcelManagement.Core.Specifications
{
    public class IncludeExpression<T>(Expression<Func<T, object>> path)
    {
        public Expression<Func<T, object>> Path { get; } = path;
        public List<Expression<Func<object, object>>> ThenIncludePaths { get; } = [];

        public IncludeExpression<T> ThenInclude(Expression<Func<object, object>> thenIncludePath)
        {
            ThenIncludePaths.Add(thenIncludePath);
            return this;
        }
    }

    public class IncludeExpressionString
    {
        public string Path { get; }

        public IncludeExpressionString(string path)
        {
            Path = path;
        }
    }

    public interface ISpecification<T>
    {
        // TODO: to remove IncludeExpressions
        List<IncludeExpression<T>> IncludeExpressions { get; }
        List<IncludeExpressionString> IncludeExpressionsString { get; }
        Expression<Func<T, bool>> ToExpression();
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDesc { get; }

        int? Page { get; }

        int? Take { get; }
    }
}