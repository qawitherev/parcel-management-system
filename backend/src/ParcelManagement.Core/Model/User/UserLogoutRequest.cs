namespace ParcelManagement.Core.Model
{
    public class UserLogoutRequest
    {
        public required Guid UserId { get; set; }
        public required string JwtId { get; set; }
    }
}