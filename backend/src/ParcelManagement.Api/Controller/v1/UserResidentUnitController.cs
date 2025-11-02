using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserResidentUnitController : ControllerBase
    {
        private readonly IUserResidentUnitService _uResidentUnitService;
        private readonly IUserContextService _userContextService;
        public UserResidentUnitController(IUserResidentUnitService uResidentUnitService, IUserContextService userContextService)
        {
            _uResidentUnitService = uResidentUnitService;
            _userContextService = userContextService;
        }

        [HttpGet("")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> GetAllUserResidentUnit([FromQuery] GetAllResidentUnitsRequestDto dto)
        {
            var filterRequest = new FilterPaginationRequest<UserResidentUnitSortableColumn>
            {
                SearchKeyword = dto.SearchKeyword,
                SortableColumn = EnumUtils.ToEnumOrNull<UserResidentUnitSortableColumn>(dto.Column ?? "") ?? UserResidentUnitSortableColumn.User,
                Page = dto.Page,
                Take = dto.Take,
                IsAscending = dto.IsAsc
            };
            var (uResidentUnits, count) = await _uResidentUnitService.GetUserResidentUnitForView(filterRequest);
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

        [HttpPatch("")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> UpdateUnitsResidents([FromBody] UpdateUnitsResidentsDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdBy = _userContextService.GetUserId();
            await _uResidentUnitService.UpdateUnitResidents([.. dto.Residents.Select(r => r.UserId)], dto.ResidentUnitId, createdBy);
            return NoContent();
        }
    }
}