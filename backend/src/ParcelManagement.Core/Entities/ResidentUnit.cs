using System.ComponentModel.DataAnnotations;

namespace ParcelManagement.Core.Entities
{
    public class ResidentUnit
    {
        [Required]
        public Guid Id { set; get; }

        [Required]
        [MaxLength(10)]
        public required string UnitName { set; get; }

        public DateTimeOffset CreatedAt { set; get; }
        public Guid CreatedBy { set; get; }

        public DateTimeOffset UpdatedAt { set; get; }
        public Guid UpdatedBy { set; get; }

        // navigation property 
        public ICollection<Parcel> Parcels { get; set; } = [];

        public ICollection<UserResidentUnit> UserResidentUnits { get; set; } = [];
    }
}