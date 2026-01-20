namespace ParcelManagement.Core.Model
{
    public class UserRefreshTokenRequest 
    {
        public required Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTimeOffset RefreshTokenExpiry { get; set; }
    }
}