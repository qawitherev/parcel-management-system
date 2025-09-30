namespace ParcelManagement.Core.Entities
{
    public enum LockerSortableColumn
    {
        Id,
        LockerName, 
        CreatedAt, 
    }
    public class Locker : IEntity
    {
        public required Guid Id { get; set; }
        public required string LockerName { get; set; }
        public required Guid CreatedBy { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public required bool IsActive { get; set; }

        // navigational properties 
        public User CreatedByUser { get; set; } = null!;
        public User? UpdatedByUser { get; set; }
        public ICollection<Parcel> Parcels { get; set; } = null!;
    }
}