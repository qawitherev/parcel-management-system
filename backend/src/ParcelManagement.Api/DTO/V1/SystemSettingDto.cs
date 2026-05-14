using ParcelManagement.Core.Entities;

namespace ParcelManagement.Api.DTO
{
    public class SystemSettingDtoResponse
    {
        public required Guid Id { get; set; }
        public string Name { get; set; } = "";
        public SystemSettingType? Type { get; set; }
        public string? Value { get; set; }
    }

    public class SystemSettingsDto
    {
        public int Count { get; set; }
        public required List<SystemSettingDtoResponse> SystemSettings { get; set; }
    }

    public class SystemSettingsDtoCreateRequest
    {
        public required string Type { get; set; }
        public required string Value { get; set; }
    }
}