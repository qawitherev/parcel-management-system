using System.Linq.Expressions;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;

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

    public class SystemSettingViewSpecification(FilterPaginationRequest<SystemSettingSortableColumn> filter) : ISpecification<SystemSetting>
    {
        public List<IncludeExpression<SystemSetting>> IncludeExpressions => [];

        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<SystemSetting, object>>? OrderBy => filter.IsAscending ? GetSortExpression() : null;

        public Expression<Func<SystemSetting, object>>? OrderByDesc => !filter.IsAscending ? GetSortExpression() : null;

        public int? Page => filter.Page;

        public int? Take => filter.Take;

        public int? Skip => null;

        public Expression<Func<SystemSetting, bool>> ToExpression()
        {
            return stm => string.IsNullOrEmpty(filter.SearchKeyword) || stm.Name.Contains(filter.SearchKeyword);
        }

        private Expression<Func<SystemSetting, object>> GetSortExpression() 
        {
            return filter.SortableColumn switch 
            {
                SystemSettingSortableColumn.Name => stm => stm.Name, 
                _ => stm => stm.Name
            };
        }
    }
}