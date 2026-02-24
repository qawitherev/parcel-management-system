using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelManagement.Api.DTO;
using ParcelManagement.Core.Entities;
using ParcelManagement.Core.Model.Helper;
using ParcelManagement.Core.Services;

namespace ParcelManagement.Api.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Consumes("application/json")]
    public class SettingController(ISystemSettingService settingService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSystemSettingById(Guid id)
        {
            var systemSetting = await settingService.GetSystemSettingByIdAsync(id);
            var dto = new SystemSettingDto
            {
                Id = systemSetting.Id,
                Name = systemSetting.Name, 
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
                
            };
            var systemSettings = await settingService.GetSystemSettings(filter);
            var dto = new SystemSettingsDto
            {
                Count = systemSettings.Count, 
                SystemSettings = [.. systemSettings.Select(sst => new SystemSettingDto {
                    Id = sst.Id,
                    Name = sst.Name, 
                    Value = sst.Value
                })]
            };
            return Ok(dto);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateSetting(SystemSettingDto dto)
        {
            var newSetting = await settingService.CreateSystemSettingAsync(dto.Name, dto.Value);
            var returnDto = new SystemSetting
            {
                Name = newSetting.Name, 
                Value = newSetting.Value
            };
            return CreatedAtAction(nameof(GetSystemSettingById), new { id = newSetting.Id}, returnDto);
        }

        [HttpPatch("")]
        public async Task<IActionResult> UpdateSetting(SystemSettingDto dto)
        {
            await settingService.UpdateSystemSettingAsync(dto.Id, dto.Value);
            return NoContent();
        }
    }
}