using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class NotificationPrefByUserIdSpecification : ISpecification<NotificationPref>
    {
        private readonly Guid _userId;

        public NotificationPrefByUserIdSpecification(Guid userId)
        {
            _userId = userId;
        }

        List<IncludeExpression<NotificationPref>> ISpecification<NotificationPref>.IncludeExpressions => [];        

        public List<IncludeExpressionString> IncludeExpressionsString => [];

        public Expression<Func<NotificationPref, object>>? OrderBy => null;

        public Expression<Func<NotificationPref, object>>? OrderByDesc => null;

        public int? Page => null;

        public int? Take => null;

        public Expression<Func<NotificationPref, bool>> ToExpression()
        {
            return np => np.UserId == _userId;
        }
    }
}