namespace ParcelManagement.Core.Entities
{
    public enum LockerSortableColumn
    {
        Id,
        LockerName, 
        CreatedAt, 
    }
    public class Locker
    {
        public required Guid Id { get; set; }
        public required string LockerName { get; set; }
        public required Guid CreatedBy { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required Guid? UpdatedBy { get; set; }
        public required DateTimeOffset? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // navigational properties 
        public User CreatedByUser { get; set; } = null!;
        public User? UpdatedByuser { get; set; }
    }
}