using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class UserByUsernameSpecification(string receivedUsername) : ISpecification<User>
    {
        public Expression<Func<User, bool>> ToExpression() => user => user.Username == receivedUsername;
    }
}