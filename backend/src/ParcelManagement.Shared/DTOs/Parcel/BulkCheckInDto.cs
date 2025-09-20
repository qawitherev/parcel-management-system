namespace ParcelManagement.Shared.DTOs.Parcel.BulkCheckInDto
{
    public class BulkCheckInRequestDto
    {
        public required string TrackingNumber { get; set; }
        public required string ResidentUnit { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimension { get; set; }
    }

    public class BulkCheckInResponseDto
    {
        public required string Status { get; set; }
        public required int ParcelCheckedIn { get; set; }
        public required string Message { get; set; }
        public List<BulkCheckInResponseErrorDto>? Error { get; set; }
    }

    public class BulkCheckInResponseErrorDto
    {
        public required int Row { get; set; }
        public required string ErrorDetail { get; set; }
    }
}