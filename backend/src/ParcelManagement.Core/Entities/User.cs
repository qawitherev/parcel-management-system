namespace ParcelManagement.Core.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public enum UserRole
    {
        Resident,
        Admin,
        ParcelRoomManager,
    }

    public class User
    {
        public Guid Id { get; set; } // Primary Key

        [Required] // Example of a data annotation for validation
        [MaxLength(100)]
        public required string Username { get; set; }

        [Required]
        [EmailAddress] // Validates that the string is a valid email format
        [MaxLength(100)]
        public required string Email { get; set; }

        //TODO - to add migration for this new column 
        [MaxLength(50)]
        public string? ResidentUnitDeprecated { get; set; }

        [Required]
        [MinLength(6)]
        public required string PasswordHash { get; set; }

        public string? PasswordSalt { get; set; }

        public UserRole Role { get; set; } // Enum for user roles

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; // Default to current time

        // navigation property
        public ICollection<UserResidentUnit> UserResidentUnits { get; set; } = [];
    }
}