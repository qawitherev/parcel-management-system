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

        [Required]
        [MaxLength(10)]
        public required string ResidentUnit { get; set; }

        public DateTimeOffset EntryDate { get; set; }
        public DateTimeOffset? ExitDate { get; set; } // Nullable if not yet claimed

        public ParcelStatus Status { get; set; }

        public decimal? Weight { get; set; } // Nullable
        public string? Dimensions { get; set; } // Nullable
        
    }
}