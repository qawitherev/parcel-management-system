
using System.ComponentModel.DataAnnotations;

namespace ParcelManagement.Core.Entities
{
    public class NotificationPref : IEntity
    {
        public Guid Id { get; set; }

        [Required]
        public required Guid UserId {get; set;}

        public bool IsEmailActive { get;set; } = true;

        public bool IsWhatsAppActive { get; set; } = true;

        public bool IsOnCheckInActive { get; set; } = true; 

        public bool IsOnClaimActive { get; set; } = false;

        public bool IsOverdueActive { get; set; } = true; 

        public DateTimeOffset? QuietHoursFrom { get; set; }

        public DateTimeOffset? QuietHoursTo { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public Guid? UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedOn { get; set; }

        public User? User { get; set; }

        public User? CreatingUser { get; set; }

        public User? UpdatingUser { get; set; }
    }

    public class UnitNotification : IEntity
    {
        [Required]
        public required Guid Id { get; set; }

        [Required]
        public required Guid UnitId { get; set; }

        [Required]
        public required Guid NotificationPrefId { get; set; }

        public bool IsActive { get; set; } = true; 

        public ResidentUnit? Unit { get; set; }

        public NotificationPref? NotificationPref { get; set; }
    }
}