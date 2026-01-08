using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class SessionByRefreshTokenSpecification(string hashedRefreshToken) : ISpecification<Session>
    {

        public List<IncludeExpressionString> IncludeExpressionsString => [
            new IncludeExpressionString("User")
        ];

        public Expression<Func<Session, object>>? OrderBy => null;

        public Expression<Func<Session, object>>? OrderByDesc => null;

        public int? Page => null;

        public int? Take => null;

        public int? Skip => null;

        List<IncludeExpression<Session>> ISpecification<Session>.IncludeExpressions => [];

        public Expression<Func<Session, bool>> ToExpression()
        {
            return s => s.RefreshToken == hashedRefreshToken;
        }
    }

    /// <summary>
    /// Specification for querying <see cref="Session"/> entities by a specific user ID,
    /// with optional support for limiting the maximum number of sessions to skip.
    /// Includes the related <see cref="User"/> entity in the query.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose sessions are being queried.</param>
    /// <param name="userMaxSession">
    /// Optional. The maximum number of sessions to skip for the user. If not provided, no sessions are skipped.
    /// </param>
    public class SessionByUserSpecification(Guid userId, int? userMaxSession) : ISpecification<Session>
    {
        public List<IncludeExpressionString> IncludeExpressionsString => [
            new IncludeExpressionString("User")
        ];

        public Expression<Func<Session, object>>? OrderBy => null;

        public Expression<Func<Session, object>>? OrderByDesc => s => s.LastActive!;

        public int? Page => null;

        public int? Take => null;

        public int? Skip => userMaxSession ?? null;

        List<IncludeExpression<Session>> ISpecification<Session>.IncludeExpressions => [];

        public Expression<Func<Session, bool>> ToExpression()
        {
            return s => s.UserId == userId;
        }
    }
}