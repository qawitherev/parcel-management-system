
using System.Linq.Expressions;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;

namespace ParcelManagement.Core.Specifications
{
    public class UserByUsernameSpecification(string receivedUsername) : ISpecification<User>
    {

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<User, object>>? OrderBy => null;

        public Expression<Func<User, object>>? OrderByDesc => null;

        List<IncludeExpression<User>> ISpecification<User>.IncludeExpressions => throw new NotImplementedException();

        public Expression<Func<User, bool>> ToExpression() => user => user.Username == receivedUsername;
    }

    public class UserForViewSpecification : ISpecification<User>
    {
        private readonly FilterPaginationRequest<UserSortableColumn> _filter;
        public UserForViewSpecification(FilterPaginationRequest<UserSortableColumn> filter)
        {
            _filter = filter;
        }
        
        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<User, object>>? OrderBy => _filter.IsAscending ? GetSortExpression() : null;

        public Expression<Func<User, object>>? OrderByDesc => _filter.IsAscending ? null : GetSortExpression();

        public int? Page => _filter.Page;

        public int? Take => _filter.Take;

        List<IncludeExpression<User>> ISpecification<User>.IncludeExpressions => throw new NotImplementedException();

        public Expression<Func<User, bool>> ToExpression()
        {
            return u => (string.IsNullOrEmpty(_filter.SearchKeyword) || (u.Username.Contains(_filter.SearchKeyword) || u.Email.Contains(_filter.SearchKeyword)))
                && u.Role == UserRole.Resident;
        }

        private Expression<Func<User, object>> GetSortExpression()
        {
            return _filter.SortableColumn switch
            {
                UserSortableColumn.Username => u => u.Username,
                UserSortableColumn.Email => u => u.Email,
                UserSortableColumn.CreatedAt => u => u.CreatedAt,
                _ => u => u.Username
            };
        }
    }

    public class UserByRefreshTokenSpecification(string refreshToken) : ISpecification<User>
    {
        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<User, object>>? OrderBy => null;

        public Expression<Func<User, object>>? OrderByDesc => null;

        public int? Page => null;

        public int? Take => null;

        List<IncludeExpression<User>> ISpecification<User>.IncludeExpressions => [];

        public Expression<Func<User, bool>> ToExpression()
        {
            return u => u.RefreshToken == refreshToken;
        }
    }
}