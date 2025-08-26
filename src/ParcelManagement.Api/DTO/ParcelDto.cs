using System.ComponentModel.DataAnnotations;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Api.DTO
{
    public class CheckInParcelDto
    {
        [Required]
        public required string TrackingNumber { get; set; }

        [Required]
        public required string ResidentUnit { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }
    }

    public class ParcelResponseDto
    {
        public required Guid Id { set; get; }
        public required string TrackingNumber { get; set; }


        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }
    }

    public class ManualEventsDto
    {
        public required string CustomEvent { get; set; }
    }

    public class ManualEventsResponseDto
    {
        public required string TrackingNumber { get; set; }
        public required TrackingEventType TrackingEventType { get; set; }
        public required string Event { get; set; }
        public required DateTimeOffset EventTime { get; set; }
    }
}