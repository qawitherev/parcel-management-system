using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class ResidentUnitByUnitNameSpecification(string unitName) : ISpecification<ResidentUnit>
    {
        public List<IncludeExpression<ResidentUnit>> IncludeExpressions => throw new NotImplementedException();

        public Expression<Func<ResidentUnit, bool>> ToExpression() => ru => ru.UnitName == unitName;
    }
}