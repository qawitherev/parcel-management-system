namespace ParcelManagement.Api.DTO
{
    public class UserResidentUnitResponseDto
    {
        public required Guid UserId { get; set; }
        public required string Username { get; set; }
        public required Guid ResidentUnitId { get; set; }
        public required String UnitName { get; set; }
    }

    public class GetAllUserResidentUnitsResponseDto
    {
        public int Count { get; set; }
        public required List<UserResidentUnitResponseDto> UserResidentUnits { get; set; }
    }

        public class GetAllUserResidentUnitsDto
    {
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
        public string? Column { get; set; }
        public int? Page { get; set; }
        public int? Take { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
