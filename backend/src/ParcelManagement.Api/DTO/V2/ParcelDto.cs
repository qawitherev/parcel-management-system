using System.ComponentModel.DataAnnotations;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Api.DTO.V2
{
    public class CheckInParcelDto
    {
        [Required]
        public required string TrackingNumber { get; set; }

        [Required]
        public required string ResidentUnit { get; set; }

        [Required]
        public required string Locker { get; set; }

        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }
    }

    public class ParcelResponseDto
    {
        public required Guid Id { set; get; }
        public required string TrackingNumber { get; set; }
        public required string Locker{ get; set; }
        public decimal? Weight { get; set; }

        public string? Dimensions { get; set; }

        public string? ResidentUnit { get; set; }
        public ParcelStatus? Status { get; set; }
    }
}