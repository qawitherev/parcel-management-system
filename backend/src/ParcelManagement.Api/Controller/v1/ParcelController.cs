
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Services;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Api.Controller.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class ParcelController(IParcelService parcelService,
        ITrackingEventService trackingEventService,
        IUserContextService userContextService
    ) : ControllerBase
    {
        private readonly IParcelService _parcelService = parcelService;
        private readonly ITrackingEventService _trackingEventService = trackingEventService;
        private readonly IUserContextService _userContextService = userContextService;

        [HttpGet("{id}")]
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

        [HttpPost("trackingNumber/{trackingNumber}/claim")]
        [Authorize(Roles = "Resident, ParcelRoomManager")]
        public async Task<IActionResult> ClaimParcel(string trackingNumber)
        {
            var userId = _userContextService.GetUserId();
            await _parcelService.ClaimParcelAsync(trackingNumber, userId);
            return NoContent(); // 204
        }

        [HttpGet("trackingNumber/{trackingNumber}")]
        [Authorize(Roles = "Resident, ParcelRoomManager")]
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

        [HttpGet("myParcels/{status?}")]
        [Authorize]
        public async Task<IActionResult> GetParcelByUser(ParcelStatus? status)
        {
            var userId = _userContextService.GetUserId();
            var (parcels, count) = await _parcelService.GetParcelByUser(userId, status);
            var responseDto = new ParcelResponseDtoList
            {
                Count = count,
                Parcels = [.. parcels.Where(p => p != null).Select(p => new ParcelResponseDto {
                    Id = p!.Id,
                    TrackingNumber = p.TrackingNumber,
                    Weight = p.Weight ?? 0,
                    Dimensions = p.Dimensions ?? ""
                })]
            };
            return Ok(responseDto);
        }

        [HttpPost("trackingNumber/{trackingNumber}/events")]
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

        [HttpGet("trackingNumber/{trackingNumber}/history")]
        [Authorize]
        public async Task<IActionResult> GetParcelHistory(string trackingNumber)
        {
            var claimedRole = _userContextService.GetUserRole();
            var res = await _parcelService.GetParcelHistoriesAsync(
                trackingNumber,
                _userContextService.GetUserId(), claimedRole
                );
            var parcelHistoriesDto = new ParcelHistoriesDto
            {
                TrackingNumber = res.TrackingNumber,
                EntryDate = res.EntryDate,
                CurrentStatus = res.Status,
                History = [.. res.TrackingEvents.Select(te => new ParcelHistoriesChild
                {
                    EventTime = te.EventTime,
                    Event = te.CustomEvent ?? te.TrackingEventType.ToString(),
                    PerformedByUser = te.User.Username
                })]
            };
            return Ok(parcelHistoriesDto);
        }

        [HttpGet("recentlyPickedUp")]
        [Authorize(Roles = "Admin, ParcelRoomManager")]
        public async Task<IActionResult> GetRecentlyPickedUp()
        {
            var (parcels, count) = await _parcelService.GetRecentlyPickedUp();
            var parcelResponseDtoList = new ParcelResponseDtoList
            {
                Count = count,
                Parcels = [.. parcels.Select(p => new ParcelResponseDto {
                    Id = p.Id,
                    TrackingNumber = p.TrackingNumber,
                    Weight = p.Weight ?? 0,
                    Dimensions = p.Dimensions ?? ""
                })]
            };
            return Ok(parcelResponseDtoList);
        }

        //TODO: to convert this into GET 
        [HttpPost("all")]
        [Authorize]
        public async Task<IActionResult> GetAllParcels([FromBody] GetAllParcelsRequestDto dto)
        {
            var role = EnumUtils.ToEnumOrNull<UserRole>(_userContextService.GetUserRole().ToString());
            var userId = _userContextService.GetUserId();
            var (resParcels, count) = await _parcelService.GetParcelsForView(
                role,
                userId,
                dto.TrackingNumber,
                EnumUtils.ToEnumOrNull<ParcelStatus>(dto.Status ?? ""),
                dto.CustomEvent,
                null,
                dto.Page,
                dto.Take
            );
            var responseDto = new GetAllParcelsResponseDto
            {
                Count = count,
                Parcels = [.. resParcels.Select(p => new ParcelResponseDto {
                    Id = p.Id,
                    TrackingNumber = p.TrackingNumber,
                    Weight = p.Weight,
                    Dimensions = p.Dimensions,
                    ResidentUnit = p.ResidentUnit!.UnitName,
                    Status = p.Status
                })],
            };
            return Ok(responseDto);
        }

        [HttpPost("bulkCheckIn")]
        [Authorize(Roles = "ParcelRoomManager, Admin")]
        public async Task<IActionResult> ParcelBulkCheckIn([FromBody] List<BulkCheckInRequestDto> parcels)
        {
            var theParcels = parcels.Select(p => (
                p.TrackingNumber,
                p.ResidentUnit,
                p.Weight,
                p.Dimension
            ));
            var userId = _userContextService.GetUserId();
            var response = await _parcelService.BulkCheckInAsync(theParcels, userId);
            var hasError = response.Items.Any(i => i.IsError);
            if (hasError)
            {
                var dtoResponseError = new BulkCheckInResponseDto
                {
                    Status = "Failed",
                    ParcelCheckedIn = 0,
                    Message = "Some parcel failed to check in. Rollback operation",
                    Error = [..response.Items.Where(i => i.IsError).Select(i => new BulkCheckInResponseErrorDto {
                    Row = i.Row, ErrorDetail = i.Message!
                })]
                };
                return BadRequest(dtoResponseError);
            }
            else
            {
                var dtoResponseSuccess = new BulkCheckInResponseDto
                {
                    Status = "Ok",
                    ParcelCheckedIn = response.Items.Count,
                    Message = "All parcels checked in",
                };
                return Ok(dtoResponseSuccess);
            }
        }
        
            
        
    }
}