namespace ParcelManagement.Api.DTO
{
    public class CheckInParcelDto
    {
        public required string TrackingNumber { get; set; }

        public required string ResidentUnit { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }
    }

    public class ParcelResponseDto
    {
        public required Guid Id { set; get; }
        public required string TrackingNumber { get; set; }

        public required string ResidentUnit { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }
    }
}