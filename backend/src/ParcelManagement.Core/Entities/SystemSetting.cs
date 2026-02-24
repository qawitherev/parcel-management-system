
namespace ParcelManagement.Core.Entities
{
    public class SystemSetting : IEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Value { get; set; }
    }
}