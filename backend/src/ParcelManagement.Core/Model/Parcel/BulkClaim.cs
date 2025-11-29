namespace ParcelManagement.Core.Model.Parcel
{
    public class BulkClaimResponse
    {
        public BulkClaimResponse()
        {
            InvalidTrackingNumbers = [];
        }

        public bool IsSuccess { get; set; }
        public int ParcelsClaimed { get; set; }
        public List<string> InvalidTrackingNumbers { get; set; }
    }
}
