namespace ParcelManagement.Api.DTO.V1
{
    public class NotificationPrefResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool IsEmailActive { get; set; }
        public bool IsWhatsAppActive { get; set; }
        public bool IsOnCheckInActive { get; set; }
        public bool IsOnClaimActive { get; set; }
        public bool IsOverdueActive { get; set; }
        public DateTimeOffset? QuiteHoursFrom { get; set; }
        public DateTimeOffset? QuiteHoursTo { get; set; }
    }

    public class NotificationPrefCreateRequestDto
    {
        public Guid UserId { get; set; }
        public bool IsEmailActive { get; set; } = true;
        public bool IsWhatsAppActive { get; set; } = true;
        public bool IsOnCheckInActive { get; set; } = true;
        public bool IsOnClaimActive { get; set; } = false;
        public bool IsOverdueActive { get; set; } = true;
        public DateTimeOffset? QuietHoursFrom { get; set; }
        public DateTimeOffset? QuietHoursTo { get; set; }
    } 
}