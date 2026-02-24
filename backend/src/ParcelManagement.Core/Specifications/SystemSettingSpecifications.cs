using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class SystemSettingByNameSpecification : ISpecification<SystemSetting>
    {
        private readonly string _name;

        public SystemSettingByNameSpecification(string name)
        {
            _name = name;
        }

        public int? Page => null;
        public int? Take => null;
        public int? Skip => null;

        public Expression<Func<SystemSetting, object>>? OrderBy => null;
        public Expression<Func<SystemSetting, object>>? OrderByDesc => null;

        public List<IncludeExpression<SystemSetting>> IncludeExpressions => [];

        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<SystemSetting, bool>> ToExpression()
        {
            return s => s.Name == _name;
        }
    }
}