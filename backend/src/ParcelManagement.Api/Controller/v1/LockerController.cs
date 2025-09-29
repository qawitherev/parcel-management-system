using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class LockerController : ControllerBase
    {
        private readonly IUserContextService _userContextService;
        private readonly ILockerService _lockerService;

        public LockerController(
            IUserContextService userContextService,
            ILockerService lockerService
        )
        {
            _userContextService = userContextService;
            _lockerService = lockerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLockerById(Guid id)
        {
            var locker = await _lockerService.GetLockerByIdAsync(id);
            var dto = new LockerResponseDto
            {
                Id = locker.Id,
                LockerName = locker.LockerName,
                CreatedBy = locker.CreatedBy.ToString(),
                CreatedAt = locker.CreatedAt,
                UpdatedBy = locker.UpdatedByUser?.Username,
                UpdatedAt = locker.UpdatedAt
            };
            return Ok(dto);
        }

        [HttpPost("")]
        [Authorize(Roles ="ParcelRoomManager, Admin")]
        public async Task<IActionResult> CreateLocker([FromBody] CreateLockerRequestDto dto)
        {
            var userId = _userContextService.GetUserId();
            var newLocker = await _lockerService.CreateLockerAsync(dto.LockerName, userId);
            var returnDto = new LockerResponseDto
            {
                Id = newLocker.Id,
                LockerName = newLocker.LockerName,
                CreatedBy = newLocker.CreatedBy.ToString(),
                CreatedAt = newLocker.CreatedAt,
                UpdatedBy = newLocker.UpdatedByUser?.Username,
                UpdatedAt = newLocker.UpdatedAt
            };
            return CreatedAtAction(nameof(GetLockerById), new { id = newLocker.Id }, returnDto);
        }
    }
}