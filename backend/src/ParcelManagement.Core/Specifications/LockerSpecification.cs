using System.Linq.Expressions;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;

namespace ParcelManagement.Core.Specifications
{
    public class LockerByLockerNameSpecification : ISpecification<Locker>
    {
        private readonly string _lockerName;
        public LockerByLockerNameSpecification(string lockerName)
        {
            _lockerName = lockerName;
            IncludeExpressionsString = [
                new IncludeExpressionString("CreatedByUser"),
                new IncludeExpressionString("UpdatedByUser")
            ];
        }

        List<IncludeExpression<Locker>> ISpecification<Locker>.IncludeExpressions => [];

        public Expression<Func<Locker, object>>? OrderBy => null;

        public Expression<Func<Locker, object>>? OrderByDesc => null;

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        public Expression<Func<Locker, bool>> ToExpression()
        {
            return l => l.LockerName == _lockerName;
        }
    }

    public class AllLockersSpecification : ISpecification<Locker>
    {
        private readonly FilterPaginationRequest<LockerSortableColumn> _filterRequest; 

        public AllLockersSpecification(
            FilterPaginationRequest<LockerSortableColumn> filterRequest)
        {
            _filterRequest = filterRequest;
            IncludeExpressionsString = [
                new IncludeExpressionString("CreatedByUser"),
                new IncludeExpressionString("UpdatedByUser")
            ];
        }

        List<IncludeExpression<Locker>> ISpecification<Locker>.IncludeExpressions => [];

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        public Expression<Func<Locker, object>>? OrderBy => _filterRequest.IsAscending ? GetSortExpression() : null;

        public Expression<Func<Locker, object>>? OrderByDesc => !_filterRequest.IsAscending ? GetSortExpression() : null;

        public int? Page => _filterRequest.Page;

        public int? Take => _filterRequest.Take;

        public Expression<Func<Locker, bool>> ToExpression()
        {
            return locker => string.IsNullOrEmpty(_filterRequest.SearchKeyword) || locker.LockerName.Contains(_filterRequest.SearchKeyword);
        }

        private Expression<Func<Locker, object>> GetSortExpression()
        {
            return _filterRequest.SortableColumn switch
            {
                LockerSortableColumn.Id => l => l.Id,
                LockerSortableColumn.LockerName => l => l.LockerName,
                _ => l => l.CreatedAt
            };
        }
    }
}