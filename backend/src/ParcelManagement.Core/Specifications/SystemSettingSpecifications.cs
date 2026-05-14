using System.Linq.Expressions;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;

namespace ParcelManagement.Core.Specifications
{
    public class SystemSettingByTypeSpecification(SystemSettingType type) : ISpecification<SystemSetting>
    {

        public int? Page => null;
        public int? Take => null;
        public int? Skip => null;

        public Expression<Func<SystemSetting, object>>? OrderBy => null;
        public Expression<Func<SystemSetting, object>>? OrderByDesc => null;

        public List<IncludeExpression<SystemSetting>> IncludeExpressions => [];

        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<SystemSetting, bool>> ToExpression()
        {
            return s => s.Type == type;
        }
    }

    public class SystemSettingViewSpecification(FilterPaginationRequest<SystemSettingSortableColumn> filter) : ISpecification<SystemSetting>
    {
        public List<IncludeExpression<SystemSetting>> IncludeExpressions => [];

        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<SystemSetting, object>>? OrderBy => null;

        public Expression<Func<SystemSetting, object>>? OrderByDesc => null;

        public int? Page => filter.Page;

        public int? Take => filter.Take;

        public int? Skip => null;

        public Expression<Func<SystemSetting, bool>> ToExpression()
        {
            return sst => true;
        }
    }
}