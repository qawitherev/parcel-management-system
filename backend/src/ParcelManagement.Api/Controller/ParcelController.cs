
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    public class ParcelController(IParcelService parcelService,
        ITrackingEventService trackingEventService,
        IUserContextService userContextService
    ) : ControllerBase
    {
        private readonly IParcelService _parcelService = parcelService;
        private readonly ITrackingEventService _trackingEventService = trackingEventService;
        private readonly IUserContextService _userContextService = userContextService;

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
        // [Authorize(Roles = "Admin, ParcelRoomManager")]
        [Authorize]
        public async Task<IActionResult> GetParcelAwaitingPickup()
        {
            var (parcels, count) = await _parcelService.GetAwaitingPickupParcelsAsync();
            var parcelResponseDtoList = new ParcelResponseDtoList
            {
                Parcels = [.. parcels
                .Where(p => p != null)
                .Select(p => new ParcelResponseDto
                {
                    Id = p!.Id,
                    TrackingNumber = p.TrackingNumber,
                    Dimensions = p.Dimensions ?? "", Weight = p.Weight ?? 0
                })],
                Count = count
            };
            return Ok(parcelResponseDtoList);
        }

        [HttpGet("myParcels")]
        [Authorize]
        public async Task<IActionResult> GetParcelByUser()
        {
            var userId = _userContextService.GetUserId();
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

        [HttpPost("{trackingNumber}/events")]
        [Authorize(Roles = "ParcelRoomManager")]
        public async Task<IActionResult> CreateManualEvent([FromBody] ManualEventsDto dto, string trackingNumber)
        {
            var performedByUser = _userContextService.GetUserId();
            var (te, p) = await _trackingEventService.ManualEventTracking(trackingNumber, performedByUser, dto.CustomEvent);
            var returnedDto = new ManualEventsResponseDto
            {
                TrackingNumber = p.TrackingNumber,
                TrackingEventType = te.TrackingEventType,
                Event = te.CustomEvent ?? "",
                EventTime = te.EventTime
            };
            return Ok(returnedDto);
        }

        [HttpGet("{trackingNumber}/history")]
        [Authorize]
        public async Task GetParcelHistory(string trackingNumber)
        {
            var res = await _parcelService.GetParcelHistoriesAsync(
                trackingNumber,
                _userContextService.GetUserId()
                );
            var parcelHistoriesDto = new ParcelHistoriesDto
            {
                TrackingNumber = res.TrackingNumber,
                History = [.. res.TrackingEvents.Select(te => new ParcelHistoriesChild
                {
                    EventTime = te.EventTime,
                    Event = te.CustomEvent ?? te.TrackingEventType.ToString(),
                    PerformedByUser = te.User.Username
                })]
            };
        }
    }
}