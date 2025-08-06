using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class ParcelsAwaitingPickupSpecification : ISpecification<Parcel>
    {
        public Expression<Func<Parcel, bool>> ToExpression() => parcel => parcel.Status == ParcelStatus.AwaitingPickup;
    }

    public class ParcelByTrackingNumberSpecification : ISpecification<Parcel>
    {
        private readonly string _trackingNumber;
        public ParcelByTrackingNumberSpecification(string trackingNumber)
        {
            _trackingNumber = trackingNumber;
        }
        public Expression<Func<Parcel, bool>> ToExpression() => p => p.TrackingNumber == _trackingNumber;
    }

    public class ParcelsByResidentUnitSpecification : ISpecification<Parcel>
    {
        private readonly string _residentUnit;
        public ParcelsByResidentUnitSpecification(string residentUnit)
        {
            _residentUnit = residentUnit;
        }
        public Expression<Func<Parcel, bool>> ToExpression() => p => p.ResidentUnit == _residentUnit;
    }

}