using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelManagement.Core.Entities
{
    public enum ParcelStatus
    {
        AwaitingPickup,
        Claimed,
        Exception
    }

    public class Parcel
    {
        public Guid Id { get; set; } // Primary Key

        [Required] // Example of a data annotation for validation
        [MaxLength(50)]
        public required string TrackingNumber { get; set; }

        [MaxLength(10)]
        public string? ResidentUnitDeprecated { get; set; }

        [Required]
        public  Guid ResidentUnitId { get; set; }

        public DateTimeOffset EntryDate { get; set; }
        public DateTimeOffset? ExitDate { get; set; } // Nullable if not yet claimed

        public ParcelStatus Status { get; set; }

        public decimal? Weight { get; set; } // Nullable
        public string? Dimensions { get; set; } // Nullable

        // navigation property 
        public ResidentUnit? ResidentUnit { get; set; }
    }
}