using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResidentUnitController : ControllerBase
    {
        private readonly IResidentUnitService _residentUnitService;
        private readonly IUserContextService _userServiceContext;

        public ResidentUnitController(
            IResidentUnitService residentUnitService,
            IUserContextService userServiceContext
            )
        {
            _residentUnitService = residentUnitService;
            _userServiceContext = userServiceContext;
        }

        [HttpGet("GetResidentUnitById/{id}")]
        public async Task<IActionResult> GetResidentUnitById(Guid id) {
            return Ok(await _residentUnitService.GetResidentUnitById(id));
        }

        [HttpPost("registerUnit")]
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
            return CreatedAtAction(nameof(GetResidentUnitById), new { id = unit.Id });
        }
    }
}