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
}