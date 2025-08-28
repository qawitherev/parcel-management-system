using System.ComponentModel.DataAnnotations;

namespace ParcelManagement.Core.Entities
{
    public enum TrackingEventType
    {
        CheckIn,
        Claim,
        Exception,
        Custom
    }

    public class TrackingEvent
    {
        [Required]
        public required Guid Id { get; set; }
        public required Guid ParcelId { get; set; }
        public required TrackingEventType TrackingEventType { get; set; }
        [MaxLength(50)]
        public string? CustomEvent { get; set; }
        public required DateTimeOffset EventTime { get; set; }
        public Guid? PerformedByUser { get; set; }

        public Parcel Parcel { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}