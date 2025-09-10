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
}