
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    public class ParcelController(IParcelService parcelService) : ControllerBase
    {
        private readonly IParcelService _parcelService = parcelService;

        // since we wont expose this endpoint publicly, we won't follow 
        // url pattern to give way to 
        // {"{trackingNumber}"}
        [HttpGet("GetParcelById/{id}")]
        public async Task<IActionResult> GetParcelById(Guid id)
        {
            var parcel = await _parcelService.GetParcelByIdAsync(id);
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
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                throw new UnauthorizedAccessException("User id is missing");
            if (!Guid.TryParse(userClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is invalid");
            }
            var newParcel = await _parcelService.CheckInParcelAsync(dto.TrackingNumber, dto.ResidentUnit, dto.Weight, dto.Dimensions, userId);
            var newParcelDto = new ParcelResponseDto
            {
                Id = newParcel.Id,
                TrackingNumber = newParcel.TrackingNumber,
                Weight = newParcel.Weight ?? 0,
                Dimensions = newParcel.Dimensions ?? ""
            };
            return CreatedAtAction(nameof(GetParcelById), new { id = newParcelDto.Id }, newParcelDto);
        }


        [HttpPost("{trackingNumber}/claim")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> ClaimParcel(string trackingNumber)
        {
            var userCLaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                throw new UnauthorizedAccessException("User id is not found");
            if (!Guid.TryParse(userCLaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("User id is invalid");
            }
            await _parcelService.ClaimParcelAsync(trackingNumber, userId);
            return NoContent(); // 204
        }

        [HttpGet("{trackingNumber}")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> GetParcelByTrackingNumber(string trackingNumber)
        {
            var resultParcel = await _parcelService.GetParcelByTrackingNumberAsync(trackingNumber);
            var resultParcelDto = new ParcelResponseDto
            {
                Id = resultParcel!.Id,
                TrackingNumber = resultParcel!.TrackingNumber,
                Weight = resultParcel!.Weight ?? 0,
                Dimensions = resultParcel!.Dimensions ?? ""
            };
            return Ok(resultParcelDto);
        }

        [HttpGet("awaitingPickup")]
        [Authorize(Roles = "Admin, ParcelRoomManager")]
        public async Task<IActionResult> GetParcelAwaitingPickup()
        {
            var parcelsAwaitingPickup = await _parcelService.GetParcelsAwaitingPickup();
            var parcelAwaitingPickupDto = parcelsAwaitingPickup.Select(entity => new ParcelResponseDto
            {
                Id = entity!.Id,
                TrackingNumber = entity.TrackingNumber,
                Weight = entity?.Weight,
                Dimensions = entity?.Dimensions
            });
            return Ok(parcelAwaitingPickupDto);
        }

        [HttpGet("myParcels")]
        [Authorize]
        public async Task<IActionResult> GetParcelByUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new UnauthorizedAccessException("User id is missing");
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid user id format");
            }
            var res = await _parcelService.GetParcelByUser(userId);
            var responseDtos = new List<ParcelResponseDto>();
            foreach (var parcel in res)
            {
                responseDtos.Add(new()
                {
                    Id = parcel!.Id,
                    TrackingNumber = parcel!.TrackingNumber,
                    Weight = parcel?.Weight ?? 0,
                    Dimensions = parcel?.Dimensions ?? "0"
                });
            }
            return Ok(responseDtos);
        }
    }
}