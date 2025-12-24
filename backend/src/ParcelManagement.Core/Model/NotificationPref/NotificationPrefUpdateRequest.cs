namespace ParcelManagement.Core.Model.NotificationPref
{
    public class NotificationPrefUpdateRequest
    {
        public required Guid NotificationPrefId { get; set; }
        public required Guid UserId { get; set; }
        public required Guid UpdatingUserId { get; set; }
        public bool IsEmailActive { get; set; }
        public bool IsWhatsAppActive { get; set; }
        public bool IsOnCheckInActive { get; set; }
        public bool IsOnClaimActive { get; set; }
        public bool IsOverdueActive { get; set; }
        public DateTimeOffset? QuietHoursFrom { get; set; }
        public DateTimeOffset? QuietHoursTo { get; set; }
    }
}