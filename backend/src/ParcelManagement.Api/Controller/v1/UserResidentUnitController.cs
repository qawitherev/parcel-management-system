using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserResidentUnitController : ControllerBase
    {
        private readonly IUserResidentUnitService _uResidentUnitService;
        public UserResidentUnitController(IUserResidentUnitService uResidentUnitService)
        {
            _uResidentUnitService = uResidentUnitService;
        }

        [HttpGet("")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> GetAllUserResidentUnit([FromQuery] GetAllResidentUnitsRequestDto dto)
        {
            var sortableColumn = EnumUtils.ToEnumOrNull<UserResidentUnitSortableColumn>(dto.Column ?? "");
            var (uResidentUnits, count) = await _uResidentUnitService.GetUserResidentUnitForView(dto.SearchKeyword, sortableColumn, dto.Page, dto.Take, dto.IsAsc);
            var responseDto = new GetAllUserResidentUnitsResponseDto
            {
                Count = count,
                UserResidentUnits = [.. uResidentUnits.Select(uru => new UserResidentUnitResponseDto {
                    UserId = uru.UserId,
                    ResidentUnitId = uru.ResidentUnitId,
                    Username = uru.User!.Username,
                    UnitName = uru.ResidentUnit!.UnitName
                })]
            };
            return Ok(responseDto);
        }
    }
}