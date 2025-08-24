
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    public class ParcelController(IParcelService parcelService) : ControllerBase
    {
        // since we wont expose this endpoint publicly, we won't follow 
        // url pattern to give way to 
        // {"{trackingNumber}"}
        [HttpGet("GetParcelById/{id}")]
        public async Task<IActionResult> GetParcelById(Guid id)
        {
            var parcel = await parcelService.GetParcelByIdAsync(id);
            return Ok(parcel);
        }

        [HttpPost("checkIn")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> CheckInParcel([FromBody] CheckInParcelDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newParcel = await parcelService.CheckInParcelAsync(dto.TrackingNumber, dto.ResidentUnit, dto.Weight, dto.Dimensions);
            var newParcelDto = new ParcelResponseDto
            {
                Id = newParcel.Id,
                TrackingNumber = newParcel.TrackingNumber,
                ResidentUnit = newParcel.ResidentUnitDeprecated!,
                Weight = newParcel.Weight ?? 0,
                Dimensions = newParcel.Dimensions ?? ""
            };
            return CreatedAtAction(nameof(GetParcelById), new { id = newParcelDto.Id }, newParcelDto);
        }


        [HttpPost("{trackingNumber}/claim")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> ClaimParcel(string trackingNumber)
        {
            await parcelService.ClaimParcelAsync(trackingNumber);
            return NoContent(); // 204
        }

        [HttpGet("{trackingNumber}")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> GetParcelByTrackingNumber(string trackingNumber)
        {
            var resultParcel = await parcelService.GetParcelByTrackingNumberAsync(trackingNumber);
            var resultParcelDto = new ParcelResponseDto
            {
                Id = resultParcel!.Id,
                TrackingNumber = resultParcel!.TrackingNumber,
                ResidentUnit = resultParcel!.ResidentUnitDeprecated!,
                Weight = resultParcel!.Weight ?? 0,
                Dimensions = resultParcel!.Dimensions ?? ""
            };
            return Ok(resultParcelDto);
        }

        [HttpGet("awaitingPickup")]
        [Authorize(Roles = "Admin, ParcelRoomManager")]
        public async Task<IActionResult> GetParcelAwaitingPickup()
        {
            var parcelsAwaitingPickup = await parcelService.GetParcelsAwaitingPickup();
            var parcelAwaitingPickupDto = parcelsAwaitingPickup.Select(entity => new ParcelResponseDto
            {
                Id = entity!.Id,
                TrackingNumber = entity.TrackingNumber,
                ResidentUnit = entity.ResidentUnitDeprecated!,
                Weight = entity?.Weight,
                Dimensions = entity?.Dimensions
            });
            return Ok(parcelAwaitingPickupDto);
        }

        // [HttpGet("myParcels")]
        // [Authorize]
        // public async Task<IActionResult> GetParcelByUser()
        // {
        //     return Ok("Not yet implemented");

        // }
    }
}