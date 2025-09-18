namespace ParcelManagement.Api.DTO
{
    public class RegisterUnitDto
    {
        public required string UnitName { get; set; }
    }

    public class AddUserToResidentUnitDto
    {
        public required Guid UserId { get; set; }
        public required Guid ResidentUnitId { get; set; }
    }

    public class GetAllResidentUnitsRequestDto
    {
        public string? UnitName { get; set; }
        public string? Column { get; set; }
        public bool IsAsc { get; set; }
        public int? Page { get; set; }
        public int? Take { get; set; }
    }

    public class ResidentUnitDto
    {
        public required Guid Id { get; set; }
        public required string UnitName { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class GetAllResidentUnitsResponseDto
    {
        public required List<ResidentUnitDto> ResidentUnits { get; set; }
        public required int Count { get; set; }
    }

    public class ResidentUnitUpdateDto
    {
        public required string UnitName { get; set; }
    }
}