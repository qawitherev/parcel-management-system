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

        public string? ResidentUnit { get; set; }
        public ParcelStatus? Status { get; set; }

    }

    public class ParcelResponseDtoList
    {
        public required List<ParcelResponseDto> Parcels { get; set; }
        public required int Count { get; set; }
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

    public class ParcelHistoriesDto
    {
        public required string TrackingNumber { get; set; }
        public required List<ParcelHistoriesChild> History { get; set; }

    }

    public class ParcelHistoriesChild
    {
        public required DateTimeOffset EventTime { get; set; }
        public required string Event { get; set; }
        public required string PerformedByUser { get; set; }
    }

    public class GetAllParcelsRequestDto
    {
        public string? TrackingNumber { get; set; }
        public string? Status { get; set; }
        public string? CustomEvent { get; set; }
        public int? Page { get; set; }
        public int? Take { get; set; }
    }

    public class GetAllParcelsResponseDto
    {
        public required List<ParcelResponseDto> Parcels { get; set; }
        public int Count { get; set; }
    }


}