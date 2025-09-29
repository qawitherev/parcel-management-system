using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;

namespace ParcelManagement.Core.Specifications
{
    public class ParcelsAwaitingPickupSpecification : ISpecification<Parcel>
    {
        public int? Page => null;

        public int? Take => null;

        List<IncludeExpressionString> ISpecification<Parcel>.IncludeExpressionsString => [];

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

        List<IncludeExpressionString> ISpecification<Parcel>.IncludeExpressionsString => [];

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

        public List<IncludeExpressionString> IncludeExpressionsString => throw new NotImplementedException();

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
            IncludeExpressionsString = [
                new IncludeExpressionString("ResidentUnit.UserResidentUnits")
            ];
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions { get; }

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

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
            IncludeExpressionsString = new List<IncludeExpressionString>()
            {
                new IncludeExpressionString("TrackingEvents.User")
            };
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions { get; }

        public int? Page => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

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

        public List<IncludeExpressionString> IncludeExpressionsString => throw new NotImplementedException();

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
        private readonly ParcelStatus? _status;
        private readonly Guid? _userId;
        private readonly UserRole? _role;
        private readonly FilterPaginationRequest<ParcelSortableColumn> _filterPaginationRequest;

        public ParcelViewSpecification(
            FilterPaginationRequest<ParcelSortableColumn> filterPaginationRequest,
            UserRole? role,
            Guid? userId,
            ParcelStatus? status
        )
        {
            _userId = userId;
            _role = role;
            _status = status;
            _filterPaginationRequest = filterPaginationRequest;
            IncludeExpressionsString = [
                new IncludeExpressionString("TrackingEvents"),
                new IncludeExpressionString("ResidentUnit.UserResidentUnits.User"),
                new IncludeExpressionString("Locker")
            ];
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions => throw new NotImplementedException();

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        public int? Page => _filterPaginationRequest.Page;

        public int? Take => _filterPaginationRequest.Take;

        public Expression<Func<Parcel, object>>? OrderBy => _filterPaginationRequest.IsAscending ? GetSortExpression() : null;

        public Expression<Func<Parcel, object>>? OrderByDesc => !_filterPaginationRequest.IsAscending ? GetSortExpression() : null;

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p =>
                (string.IsNullOrEmpty(_filterPaginationRequest.SearchKeyword) || p.TrackingNumber.Contains(_filterPaginationRequest.SearchKeyword) || 
                    (p.Locker != null && p.Locker.LockerName.Contains(_filterPaginationRequest.SearchKeyword)) ||
                    p.TrackingEvents.Any(te => !string.IsNullOrEmpty(te.CustomEvent) && te.CustomEvent.Contains(_filterPaginationRequest.SearchKeyword))
                ) &&
                (!_status.HasValue || p.Status == _status) &&
                (_role != UserRole.Resident || p.ResidentUnit!.UserResidentUnits.Any(uru => _userId.HasValue && uru.UserId == _userId.Value));
        }

        private Expression<Func<Parcel, object>> GetSortExpression()
        {
            return _filterPaginationRequest.SortableColumn switch
            {
                ParcelSortableColumn.TrackingNumber => p => p.TrackingNumber,
                _ => p => p.EntryDate
            };
        }
    }

    public class ParcelDetailsByIdSpecification : ISpecification<Parcel>
    {
        private readonly Guid _id;
        public ParcelDetailsByIdSpecification(Guid id)
        {
            _id = id;
            IncludeExpressionsString = [
                new IncludeExpressionString("ResidentUnit"),
                new IncludeExpressionString("Locker")
            ];
        }
        
        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        Expression<Func<Parcel, object>>? ISpecification<Parcel>.OrderBy => null;

        Expression<Func<Parcel, object>>? ISpecification<Parcel>.OrderByDesc => null;

        int? ISpecification<Parcel>.Page => null;

        int? ISpecification<Parcel>.Take => null;

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p => p.Id == _id;
        }
    }
}