using System.Linq.Expressions;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;

namespace ParcelManagement.Core.Specifications
{
    public class UserResidentUnitByUserIdSpecification(Guid userId) : ISpecification<UserResidentUnit>
    {
        public List<IncludeExpression<UserResidentUnit>> IncludeExpressions => throw new NotImplementedException();

        public int? Page => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

        public List<IncludeExpressionString> IncludeExpressionsString => throw new NotImplementedException();

        public Expression<Func<UserResidentUnit, object>>? OrderBy => throw new NotImplementedException();

        public Expression<Func<UserResidentUnit, object>>? OrderByDesc => throw new NotImplementedException();

        public Expression<Func<UserResidentUnit, bool>> ToExpression() => uru => uru.UserId == userId;
    }

    public class UserResidentUnitByResidentUnitIdSpecification(Guid residentUnitId) :
        ISpecification<UserResidentUnit>
    {
        public List<IncludeExpression<UserResidentUnit>> IncludeExpressions => throw new NotImplementedException();

        public int? Page => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

        public List<IncludeExpressionString> IncludeExpressionsString => throw new NotImplementedException();

        public Expression<Func<UserResidentUnit, object>>? OrderBy => throw new NotImplementedException();

        public Expression<Func<UserResidentUnit, object>>? OrderByDesc => throw new NotImplementedException();

        public Expression<Func<UserResidentUnit, bool>> ToExpression()
        {
            return uru => uru.ResidentUnitId == residentUnitId;
        }
    }

    public class UserResidentUnitUnitViewSpecification : ISpecification<UserResidentUnit>
    {
        private readonly FilterPaginationRequest<UserResidentUnitSortableColumn> _filterPaginationRequest;
        public UserResidentUnitUnitViewSpecification(
            FilterPaginationRequest<UserResidentUnitSortableColumn> filterPaginationRequest
        )
        {
            _filterPaginationRequest = filterPaginationRequest;
            IncludeExpressionsString = [
                new IncludeExpressionString("User"),
                new IncludeExpressionString("ResidentUnit"),
            ];

        }

        List<IncludeExpression<UserResidentUnit>> ISpecification<UserResidentUnit>.IncludeExpressions => throw new NotImplementedException();

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        public Expression<Func<UserResidentUnit, object>>? OrderBy => _filterPaginationRequest.IsAscending ? GetSortExpression() : null;

        public Expression<Func<UserResidentUnit, object>>? OrderByDesc => _filterPaginationRequest.IsAscending ? null : GetSortExpression();

        public int? Page => _filterPaginationRequest.Page;

        public int? Take => _filterPaginationRequest.Take;

        public Expression<Func<UserResidentUnit, bool>> ToExpression()
        {
            return uResidentUnit => (string.IsNullOrEmpty(_filterPaginationRequest.SearchKeyword) || uResidentUnit.ResidentUnit!.UnitName.Contains(_filterPaginationRequest.SearchKeyword))
                &&
                (string.IsNullOrEmpty(_filterPaginationRequest.SearchKeyword) || uResidentUnit.User!.Username.Contains(_filterPaginationRequest.SearchKeyword)) &&
                uResidentUnit.IsActive;
        }

        private Expression<Func<UserResidentUnit, object>> GetSortExpression()
        {
            return _filterPaginationRequest.SortableColumn switch
            {
                UserResidentUnitSortableColumn.User => uru => uru.User!,
                UserResidentUnitSortableColumn.ResidentUnit => uru => uru.ResidentUnit!,
                _ => uru => uru.User!
            };
        }
    }
}