namespace ParcelManagement.Core.Model.Configuration {
    public class JWTSettings
    {
        public string? SecretKey { get; set; }

        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public int? ExpirationMinutes { get; set; }
    }
}