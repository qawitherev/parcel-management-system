namespace ParcelManagement.Api.DTO
{
    public class SystemSettingDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Value { get; set; }
    }

    public class SystemSettingsDto
    {
        public int Count { get; set; }
        public required List<SystemSettingDto> SystemSettings { get; set; }
    }
}