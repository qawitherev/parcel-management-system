using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Api.Utility;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class SettingController(ISystemSettingService settingService, IParcelService parcelService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSystemSettingById(Guid id)
        {
            var systemSetting = await settingService.GetSystemSettingByIdAsync(id);
            var dto = new SystemSettingDtoResponse
            {
                Id = systemSetting.Id,
                Type = systemSetting.Type,
                Value = systemSetting.Value
            };
            return Ok(dto);
        }

        [HttpGet("")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSettings()
        {
            var filter = new FilterPaginationRequest<SystemSettingSortableColumn>
            {
                // i think no need to pass anything here 
            };
            var systemSettings = await settingService.GetSystemSettings(filter);
            var dto = new SystemSettingsDto
            {
                Count = systemSettings.Count, 
                SystemSettings = [.. systemSettings.Select(sst => new SystemSettingDtoResponse {
                    Id = sst.Id,
                    Value = sst.Value
                })]
            };
            return Ok(dto);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateSetting(SystemSettingsDtoCreateRequest dto)
        {
            var type = EnumUtils.ToEnumOrNull<SystemSettingType>(dto.Type) ?? throw new InvalidCastException("Invalid setting type");
            var newSetting = await settingService.CreateSystemSettingAsync(type, dto.Value);
            var returnDto = new SystemSetting
            {
                Name = newSetting.Name, 
                Value = newSetting.Value
            };
            return CreatedAtAction(nameof(GetSystemSettingById), new { id = newSetting.Id}, returnDto);
        }

        [HttpPatch("")]
        public async Task<IActionResult> UpdateSetting(SystemSettingDtoResponse dto)
        {
            await settingService.UpdateSystemSettingAsync(dto.Id, dto.Value);
            await parcelService.UpdateOverstayedParcel();
            return NoContent();
        }
    }
}