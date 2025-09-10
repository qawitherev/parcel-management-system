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
        Expression<Func<T, bool>> ToExpression();
        List<IncludeExpression<T>> IncludeExpressions { get; }

        List<IncludeExpressionString> IncludeExpressionString { get; }


        int? Skip { get; }

        int? Take { get; }
    }
}