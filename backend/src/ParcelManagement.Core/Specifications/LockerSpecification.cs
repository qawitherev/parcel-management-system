using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

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
}