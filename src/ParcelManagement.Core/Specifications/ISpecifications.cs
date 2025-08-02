using System.Linq.Expressions;

namespace ParcelManagement.Core.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression(); 
    }
}