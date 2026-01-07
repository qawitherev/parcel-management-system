using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class SessionByRefreshTokenSpecification(string refreshToken) : ISpecification<Session>
    {
        private readonly string _refreshToken = refreshToken;

        public List<IncludeExpressionString> IncludeExpressionsString => [
            new IncludeExpressionString("User")
        ];

        public Expression<Func<Session, object>>? OrderBy => null;

        public Expression<Func<Session, object>>? OrderByDesc => null;

        public int? Page => null;

        public int? Take => null;

        List<IncludeExpression<Session>> ISpecification<Session>.IncludeExpressions => [];

        public Expression<Func<Session, bool>> ToExpression()
        {
            return s => s.RefreshToken == _refreshToken;
        }
    }
}