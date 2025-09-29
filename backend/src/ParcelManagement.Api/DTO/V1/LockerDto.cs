using System.Diagnostics.CodeAnalysis;
using ParcelManagement.Core.Specifications;

namespace ParcelManagement.Api.DTO.V1
{
    public class CreateLockerRequestDto
    {
        public required string LockerName { get; set; }
    }


    public class LockerResponseDto
    {
        public required Guid Id { get; set; }
        public required string LockerName { get; set; }
        public required string CreatedBy { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class GetAllLockersRequestDto
    {
        public string? SearchKeyword { get; set; }
        public string? Status { get; set; }
        public string? Column { get; set; }
        public int? Page { get; set; }
        public int? Take { get; set; }
        public bool IsAscending { get; set; } = true;
    }

    public class GetAllLockersResponseDto
    {
        public int Count { get; set; }
        public required List<LockerResponseDto> Lockers { get; set; }
    }

    public class UpdateLockerRequestDto
    {
        public required string LockerName { get; set; }
    }
}