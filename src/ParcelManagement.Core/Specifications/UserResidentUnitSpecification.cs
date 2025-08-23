using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class UserResidentUnitByUserIdSpecification(Guid userId) : ISpecification<UserResidentUnit>
    {
        public Expression<Func<UserResidentUnit, bool>> ToExpression() => uru => uru.UserId == userId;
    }

    public class UserResidentUnitByResidentUnitIdSpecification(Guid residentUnitId) :
        ISpecification<UserResidentUnit>
    {
        public Expression<Func<UserResidentUnit, bool>> ToExpression()
        {
            return uru => uru.ResidentUnitId == residentUnitId;
        }
    }
}