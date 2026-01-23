namespace ParcelManagement.Core.Model
{
    public class CreateSessionRequest
    {
        public required Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}