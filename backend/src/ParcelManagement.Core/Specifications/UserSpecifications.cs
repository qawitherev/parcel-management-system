
using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class UserByUsernameSpecification(string receivedUsername) : ISpecification<User>
    {
        public List<IncludeExpression<User>> IncludeExpressions => throw new NotImplementedException();

        public int? Skip => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString => throw new NotImplementedException();

        public Expression<Func<User, bool>> ToExpression() => user => user.Username == receivedUsername;
    }
}