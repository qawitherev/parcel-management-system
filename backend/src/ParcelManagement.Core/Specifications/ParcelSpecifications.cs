using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class ParcelsAwaitingPickupSpecification : ISpecification<Parcel>
    {
        public int? Skip => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString => throw new NotImplementedException();

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

        public int? Skip => null;

        public int? Take => null;

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

        public int? Skip => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString => throw new NotImplementedException();

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

        public int? Skip => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString { get; }

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

        public int? Skip => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString { get; } 

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p => p.TrackingEvents.Any(te => te.ParcelId == _parcelId);
        }
    }

    public class ParcelRecentlyPickedUpSpecification : ISpecification<Parcel>
    {
        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        public int? Skip => null;

        public int? Take => null;

        public List<IncludeExpressionString> IncludeExpressionString => throw new NotImplementedException();

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            var threeDaysAgo = DateTimeOffset.UtcNow.AddDays(-3);
            return p =>
                p.Status == ParcelStatus.Claimed && p.EntryDate >= threeDaysAgo;
        }
    }
}