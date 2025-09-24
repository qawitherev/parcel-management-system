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
}