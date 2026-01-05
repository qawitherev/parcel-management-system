namespace ParcelManagement.Core.Model.User
{
    public class UserLoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string RefreshToken { get; set; } = "";
        public DateTimeOffset? RefreshTokenExpiry { get; set; }
    }
}