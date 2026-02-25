
namespace ParcelManagement.Core.Entities
{
    public enum SystemSettingSortableColumn
    {
        Name
    }

    public enum SystemSettingType
    {
        OverstayThresholdDays
    }

    public class SystemSetting : IEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public SystemSettingType Type { get; set; }
        public string? Value { get; set; }
    }
}