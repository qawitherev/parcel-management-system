namespace ParcelManagement.Core.Model.NotificationPref
{
    public class NotificationPrefCreateRequest
    {
        public required Guid UserId { get; set; }
        
        public required Guid CreatingUserId { get; set; }

        public bool IsEmailActive { get; set; } = true; 

        public bool IsWhatsAppActive { get; set; } = true; 

        public bool IsOnCheckInActive { get; set; } = true; 

        public bool IsOnClaimActive { get; set; } = false; 

        public bool IsOverdueActive { get; set; } = true; 

        public DateTimeOffset? QuietHoursFrom { get; set; }

        public DateTimeOffset? QuietHoursTo { get; set; }
    }
}