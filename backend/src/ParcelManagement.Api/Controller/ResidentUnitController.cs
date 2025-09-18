using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    public class ResidentUnitController : ControllerBase
    {
        private readonly IResidentUnitService _residentUnitService;
        private readonly IUserContextService _userServiceContext;
        private readonly IUserResidentUnitService _uruService;

        public ResidentUnitController(
            IResidentUnitService residentUnitService,
            IUserContextService userServiceContext,
            IUserResidentUnitService uruService
            )
        {
            _residentUnitService = residentUnitService;
            _userServiceContext = userServiceContext;
            _uruService = uruService;
        }

        [HttpGet("GetResidentUnitById/{id}")]
        public async Task<IActionResult> GetResidentUnitById(Guid id)
        {
            return Ok(await _residentUnitService.GetResidentUnitById(id));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> GetResidentUnitByIdReal(Guid id)
        {
            return Ok(await _residentUnitService.GetResidentUnitById(id));
        }

        [HttpPost("registerUnit")]
        [Consumes("application/json")]
        public async Task<IActionResult> RegisterUnit([FromBody] RegisterUnitDto registerUnitDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = _userServiceContext.GetUserId();
            var unit = await _residentUnitService.CreateResidentUnitAsync(
                registerUnitDto.UnitName, userId
            );
            return CreatedAtAction(nameof(GetResidentUnitById), new { id = unit.Id }, unit);
        }

        [HttpPost("addUser")]
        [Consumes("application/json")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> AddUserToResidentUnit([FromBody] AddUserToResidentUnitDto dto)
        {
            var creatorId = _userServiceContext.GetUserId();
            await _uruService.CreateUserResidentUnit(creatorId, dto.UserId, dto.ResidentUnitId);
            var residentUnit = await _residentUnitService.GetResidentUnitById(dto.ResidentUnitId);
            return CreatedAtAction(nameof(GetResidentUnitById), new { id = dto.ResidentUnitId }, residentUnit);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin, ParcelRoomManager")]
        public async Task<IActionResult> GetAllResidentUnit([FromQuery] GetAllResidentUnitsRequestDto dto)
        {
            var column = EnumUtils.ToEnumOrNull<ResidentUnitSortableColumn>(dto.Column ?? "");
            var (residentUnits, count) = await _residentUnitService.GetResidentUnitsForViewAsync(dto.UnitName, column, dto.Page, dto.Take, dto.IsAsc);
            var responseDto = new GetAllResidentUnitsResponseDto
            {
                ResidentUnits = [.. residentUnits.Select(ru => new ResidentUnitDto {
                    Id = ru.Id,
                    UnitName = ru.UnitName,
                    CreatedAt = ru.CreatedAt,
                    CreatedBy = ru.CreatedByUser.Username ?? "",
                    UpdatedAt = ru.UpdatedAt,
                    UpdatedBy = ru.UpdatedByUser?.Username ?? ""
                })],
                Count = count
            };
            return Ok(responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResidentUnitAsync([FromBody] ResidentUnitUpdateDto dto, Guid id)
        {
            var userId = _userServiceContext.GetUserId();
            await _residentUnitService.UpdateResidentUnitAsync(id, dto.UnitName, userId);
            return NoContent();
        }
    }
}