namespace ParcelManagement.Core.Model.Parcel
{
    public class BulkCheckInResponse
    {
        public BulkCheckInResponse()
        {
            Items = [];
        }

        public required List<ParcelCheckInResponse> Items { get; set; }
    }

    public class ParcelCheckInResponse
    {
        public required string TrackingNumber { get; set; }
        public required int Row { get; set; }
        public required bool IsError { get; set; }
        public string? Message { get; set; }
    }
}