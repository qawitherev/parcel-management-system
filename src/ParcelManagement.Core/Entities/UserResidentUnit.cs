namespace ParcelManagement.Core.Entities
{
    public class UserResidentUnit
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ResidentUnitId { get; set; }

        public Boolean IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        // navigation property
        public User? User { get; set; }

        public ResidentUnit? ResidentUnit { get; set; }
    }
}