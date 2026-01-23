
using System.ComponentModel.DataAnnotations;

namespace ParcelManagement.Core.Entities
{
    public class Session : IEntity
    {
        [Required]
        public required Guid Id { get; set; }
        [Required]
        public required Guid UserId { get; set; }
        public string? RefreshToken {get; set; }
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public DateTimeOffset? ExpiresAt {get; set; }
        public DateTimeOffset? LastActive {get; set; }

        // navigational properties 
        public User? User { get; set; }

    }
}