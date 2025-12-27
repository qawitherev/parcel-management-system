using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO.V1;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Model.NotificationPref;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]

    public class NotificationPrefController : ControllerBase
    {
        private readonly INotificationPrefService _notiPrefService;
        private readonly IUserContextService _userContextService; 
        public NotificationPrefController(INotificationPrefService notificationPrefService,
            IUserContextService userContextService)
        {
            _notiPrefService = notificationPrefService;
            _userContextService = userContextService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetNotificationPrefById(Guid id)
        {
            var notiPref = await _notiPrefService.GetNotificationPrefByIdAsync(id);
            var returnDto = new NotificationPrefResponseDto
            {
                Id = notiPref!.Id,
                UserId = notiPref.UserId,
                IsEmailActive = notiPref.IsEmailActive,
                IsWhatsAppActive = notiPref.IsWhatsAppActive,
                IsOnCheckInActive = notiPref.IsOnCheckInActive,
                IsOnClaimActive = notiPref.IsOnClaimActive,
                IsOverdueActive = notiPref.IsOverdueActive,
                QuietHoursFrom = notiPref.QuietHoursFrom, 
                QuietHoursTo = notiPref.QuietHoursTo
            };
            return Ok(returnDto);
        }

        [HttpPost("")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> CreateNotificationPref([FromBody] NotificationPrefCreateRequestDto payload) 
        {
            var userId = _userContextService.GetUserId();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var np = new NotificationPrefCreateRequest
            {
                UserId = userId, 
                CreatingUserId = userId, 
                IsEmailActive = payload.IsEmailActive,
                IsWhatsAppActive = payload.IsWhatsAppActive,
                IsOnCheckInActive = payload.IsOnCheckInActive,
                IsOnClaimActive = payload.IsOnClaimActive,
                IsOverdueActive = payload.IsOverdueActive,
                QuietHoursFrom = payload.QuietHoursFrom, 
                QuietHoursTo = payload.QuietHoursTo
            };
            var newNp = await _notiPrefService.CreateNotificationPrefAsync(np);
            return CreatedAtAction(nameof(GetNotificationPrefById), new { id = newNp.Id}, newNp);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> GetNotificationPrefByUser()
        {
            var userId = _userContextService.GetUserId();
            var np = await _notiPrefService.GetNotificationPrefByUserId(userId);
            var returnDto = new NotificationPrefResponseDto
            {
                Id = np!.Id,
                UserId = np.UserId,
                IsEmailActive = np.IsEmailActive,
                IsWhatsAppActive = np.IsWhatsAppActive,
                IsOnCheckInActive = np.IsOnCheckInActive,
                IsOnClaimActive = np.IsOnClaimActive,
                IsOverdueActive = np.IsOverdueActive,
                QuietHoursFrom = np.QuietHoursFrom, 
                QuietHoursTo = np.QuietHoursTo
            };
            return Ok(returnDto);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Resident")]
        public async Task<IActionResult> UpdateNotificationPref(Guid id, [FromBody] NotificationPrefUpdateRequestDto requestDto )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatingUserId = _userContextService.GetUserId();
            var payload = new NotificationPrefUpdateRequest
            {
                NotificationPrefId = id,
                UserId = updatingUserId,
                UpdatingUserId = updatingUserId,
                IsEmailActive = requestDto.IsEmailActive,
                IsWhatsAppActive = requestDto.IsWhatsAppActive,
                IsOnCheckInActive = requestDto.IsOnCheckInActive,
                IsOnClaimActive = requestDto.IsOnClaimActive,
                IsOverdueActive = requestDto.IsOverdueActive,
                QuietHoursFrom = requestDto.QuietHoursFrom, 
                QuietHoursTo = requestDto.QuietHoursTo
            };
            await _notiPrefService.UpdateNotificationPrefs(payload, updatingUserId);
            return NoContent();
        }
    }
}