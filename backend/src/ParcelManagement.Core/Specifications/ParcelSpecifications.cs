using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class ParcelsAwaitingPickupSpecification : ISpecification<Parcel>
    {
        public int? Page => null;

        public int? Take => null;

        List<IncludeExpressionString> ISpecification<Parcel>.IncludeExpressionString => [];

        Expression<Func<Parcel, object>>? ISpecification<Parcel>.OrderBy => null;

        Expression<Func<Parcel, object>>? ISpecification<Parcel>.OrderByDesc => null;

        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        public Expression<Func<Parcel, bool>> ToExpression() => parcel => parcel.Status == ParcelStatus.AwaitingPickup;
    }

    public class ParcelByTrackingNumberSpecification : ISpecification<Parcel>
    {
        private readonly string _trackingNumber;
        public ParcelByTrackingNumberSpecification(string trackingNumber)
        {
            _trackingNumber = trackingNumber;
        }

        public int? Page => null;

        public int? Take => null;

        public Expression<Func<Parcel, object>> OrderBy => throw new NotImplementedException();

        public Expression<Func<Parcel, object>> OrderByDesc => throw new NotImplementedException();

        List<IncludeExpressionString> ISpecification<Parcel>.IncludeExpressionString => [];

        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        public Expression<Func<Parcel, bool>> ToExpression() => p => p.TrackingNumber == _trackingNumber;
    }

    public class ParcelsByResidentUnitSpecification : ISpecification<Parcel>
    {
        private readonly string _residentUnit;
        public ParcelsByResidentUnitSpecification(string residentUnit)
        {
            _residentUnit = residentUnit;
        }

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString => throw new NotImplementedException();

        public Expression<Func<Parcel, object>> OrderBy => throw new NotImplementedException();

        public Expression<Func<Parcel, object>> OrderByDesc => throw new NotImplementedException();

        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        public Expression<Func<Parcel, bool>> ToExpression() => p => p.ResidentUnitDeprecated == _residentUnit;
    }

    public class ParcelByUserSpecification : ISpecification<Parcel>
    {
        private readonly Guid _userId;
        private readonly ParcelStatus? _status;
        public ParcelByUserSpecification(Guid userId, ParcelStatus? status = null)
        {
            _userId = userId;
            _status = status;
            IncludeExpressions = new List<IncludeExpression<Parcel>>
            {
                new IncludeExpression<Parcel>(p => p.ResidentUnit!)
                    .ThenInclude(ru => ((ResidentUnit)ru).UserResidentUnits)
            };
            IncludeExpressionString = [
                new IncludeExpressionString("ResidentUnit.UserResidentUnits")
            ];
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions { get; }

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString { get; }

        Expression<Func<Parcel, object>>? ISpecification<Parcel>.OrderBy => null;

        Expression<Func<Parcel, object>>? ISpecification<Parcel>.OrderByDesc => null;

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p => p.ResidentUnit!.UserResidentUnits.Any(uru => uru.UserId == _userId)
                && (!_status.HasValue || p.Status == _status);
        }
    }

    public class ParcelHistoriesSpecification : ISpecification<Parcel>
    {
        private readonly Guid _parcelId;

        public ParcelHistoriesSpecification(Guid parcelId)
        {
            _parcelId = parcelId;
            IncludeExpressions = new List<IncludeExpression<Parcel>>
            {
                new IncludeExpression<Parcel>(p => p.TrackingEvents)
            };
            IncludeExpressionString = new List<IncludeExpressionString>()
            {
                new IncludeExpressionString("TrackingEvents.User")
            };
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions { get; }

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString { get; }

        public Expression<Func<Parcel, object>> OrderBy => throw new NotImplementedException();

        public Expression<Func<Parcel, object>> OrderByDesc => throw new NotImplementedException();

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p => p.TrackingEvents.Any(te => te.ParcelId == _parcelId);
        }
    }

    public class ParcelRecentlyPickedUpSpecification : ISpecification<Parcel>
    {
        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString => throw new NotImplementedException();

        public Expression<Func<Parcel, object>> OrderBy => throw new NotImplementedException();

        public Expression<Func<Parcel, object>> OrderByDesc => throw new NotImplementedException();

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            var threeDaysAgo = DateTimeOffset.UtcNow.AddDays(-3);
            return p =>
                p.Status == ParcelStatus.Claimed && p.EntryDate >= threeDaysAgo;
        }
    }

    public class ParcelViewSpecification : ISpecification<Parcel>
    {
        private readonly string? _trackingNumber;
        private readonly int? _page;
        private readonly int? _take;
        private readonly ParcelStatus? _status;
        private readonly string? _customEvent;
        private readonly Guid? _userId;
        private readonly UserRole? _role;
        private readonly ParcelSortableColumn? _column;
        private readonly bool _isAsc;

        public ParcelViewSpecification(
            UserRole? role,
            Guid? userId,
            string? trackingNumber,
            ParcelStatus? status,
            string? customEvent,
            ParcelSortableColumn? column,
            int? page,
            int? take = 20,
            bool isAsc = true

        )
        {
            _userId = userId;
            _role = role;
            _trackingNumber = trackingNumber;
            _status = status;
            _customEvent = customEvent;
            _column = column;
            _isAsc = isAsc;
            _page = page;
            _take = take;
            IncludeExpressionString = [
                new IncludeExpressionString("TrackingEvents"),
                new IncludeExpressionString("ResidentUnit.UserResidentUnits.User")
            ];
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions => throw new NotImplementedException();

        public List<IncludeExpressionString> IncludeExpressionString { get; }

        public int? Page => _page;

        public int? Take => _take;

        public Expression<Func<Parcel, object>>? OrderBy => _isAsc ? GetSortExpression() : null;

        public Expression<Func<Parcel, object>>? OrderByDesc => !_isAsc ? GetSortExpression() : null;

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p =>
                (string.IsNullOrEmpty(_trackingNumber) || p.TrackingNumber.Contains(_trackingNumber)) &&
                (!_status.HasValue || p.Status == _status) &&
                (string.IsNullOrEmpty(_customEvent) || p.TrackingEvents.Any(te => !string.IsNullOrEmpty(te.CustomEvent) && te.CustomEvent.Contains(_customEvent))) &&
                (_role != UserRole.Resident || p.ResidentUnit!.UserResidentUnits.Any(uru => _userId.HasValue && uru.UserId == _userId.Value));
        }

        private Expression<Func<Parcel, object>> GetSortExpression()
        {
            return _column switch
            {
                ParcelSortableColumn.TrackingNumber => p => p.TrackingNumber,
                _ => p => p.EntryDate
            };
        }
    }
}