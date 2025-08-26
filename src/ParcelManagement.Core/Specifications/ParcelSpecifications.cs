using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class ParcelsAwaitingPickupSpecification : ISpecification<Parcel>
    {
        public int? Skip => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

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

        public int? Skip => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

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

        public int? Skip => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

        List<IncludeExpression<Parcel>> ISpecification<Parcel>.IncludeExpressions => [];

        public Expression<Func<Parcel, bool>> ToExpression() => p => p.ResidentUnitDeprecated == _residentUnit;
    }

    public class ParcelByUserSpecification : ISpecification<Parcel>
    {
        private readonly Guid _userId;
        public ParcelByUserSpecification(Guid userId)
        {
            _userId = userId;
            IncludeExpressions = new List<IncludeExpression<Parcel>>
            {
                new IncludeExpression<Parcel>(p => p.ResidentUnit!)
                    .ThenInclude(ru => ((ResidentUnit)ru).UserResidentUnits)
            };
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions { get; }

        public int? Skip => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p => p.ResidentUnit!.UserResidentUnits.Any(uru => uru.UserId == _userId);
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
        }

        public List<IncludeExpression<Parcel>> IncludeExpressions { get; }

        public int? Skip => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

        public Expression<Func<Parcel, bool>> ToExpression()
        {
            return p => p.TrackingEvents.Any(te => te.ParcelId == _parcelId);
        }
    }
}