using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.Controller.V1;
using ParcelManagement.Api.DTO.V2;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class ParcelController(
        IParcelService parcelService,
        IUserContextService userContextService
    ) : ControllerBase
    {
        private readonly IParcelService _parcelService = parcelService;
        private readonly IUserContextService _userContextService = userContextService;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParcelById(Guid id)
        {
            var parcel = await _parcelService.GetParcelByIdAsync(id);
            return Ok(parcel);
        }

        [HttpPost("checkIn")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInParcelDto dto)
        {
            var userId = _userContextService.GetUserId();
            var newParcel = await _parcelService.CheckInParcelWithLockerAsync(
                dto.TrackingNumber,
                dto.ResidentUnit,
                dto.Locker,
                dto.Weight,
                dto.Dimensions,
                userId);
            var parcelDto = new ParcelResponseDto
            {
                Id = newParcel.Id,
                TrackingNumber = newParcel.TrackingNumber,
                Locker = newParcel.Locker!.LockerName,
                Weight = newParcel.Weight,
                Dimensions = newParcel.Dimensions
            };

            return CreatedAtAction(nameof(GetParcelById), new { id = newParcel.Id }, parcelDto);
        }
        
        
    }
}