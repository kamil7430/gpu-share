using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Infrastructure.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GpuShare.Frontend.Services
{
    public class DeviceService(IApiClient api, ILogger<DeviceService> logger) : IDeviceService
    {
        private readonly IApiClient _api = api;
        private readonly ILogger<DeviceService> _logger = logger;

        public async Task<Device> GetDeviceAsync(int deviceId)
        {
            var device = await _api.GetAsync<Device>($"/api/devices/{deviceId}");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got device with id {Id}", deviceId);
            return device!;
        }

        public async Task<DeviceStatus> GetDeviceStatusAsync(int deviceId)
        {
            var deviceStatus = await _api.GetAsync<DeviceStatus>($"/api/devices/{deviceId}/status");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got status device with id {id}. Device is {health}", deviceId, deviceStatus!.Online);
            return deviceStatus!;
        }

        public async Task<Device> RegisterDeviceAsync(RegisterDeviceRequest cmd)
        {
            var response = await _api.PostAsync<RegisterDeviceRequest, RegisterDeviceResponse>($"/api/devices", cmd);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Registered device with name {name}. Got ID {id} for it.", cmd.Name, response!.DeviceId);
            
            return new Device { 
                DeviceId = response!.DeviceId, 
                Name = cmd.Name,
                GpuModel = cmd.GpuModel,
                VramMb = cmd.VramMb,
                CudaCores = cmd.CudaCores,
                DriverVersion = cmd.DriverVersion,
                Frameworks = cmd.Frameworks,
                PricePerHourUsdCents = cmd.PricePerHourUsdCents,
                OwnerUsername = response.OwnerUsername,
                State = response.State,
            };
        }

        public async Task DeleteDeviceAsync(int deviceId)
        {
            await _api.DeleteAsync($"/api/devices/{deviceId}");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Deleted device with ID {id}.", deviceId);
        }

        public async Task<PagedResult<Device>> SearchDevicesAsync(DeviceSearchFilters filters)
        {
            var devices = await _api.GetAsync<List<Device>>($"/api/devices", filters);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"Searching for '{filters.Name}', " +
                    $"price between {filters.MinPricePerHourUsdCents ?? 0} " +
                    $"and {filters.MinPricePerHourUsdCents ?? 0}, " +
                    $"cores between {filters.MinCudaCores ?? 0} and {filters.MaxCudaCores ?? 0}, " +
                    $"available only: {filters.AvailableOnly}, limit: {filters.Limit}");
            
            return new PagedResult<Device>
            {
                Items = devices!,
                TotalCount = devices!.Count,
                Page = 1,
                PageSize = devices!.Count
            };
        }
        public async Task<List<Device>> GetUserDevicesAsync(string username)
        {
            var devices = await _api.GetAsync<List<Device>>($"/api/users/{username}/devices");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got devices for user {username}.", username);

            return devices!;
        }

        public async Task<Device> UpdateDeviceAsync(int deviceId, Device oldDevice, UpdateDeviceRequest cmd)
        {
            await _api.PatchAsync<UpdateDeviceRequest>($"/api/devices/{deviceId}", cmd);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Updated device with ID {id}.", deviceId);
            
            return new Device { 
                DeviceId = deviceId, 
                Name = cmd.Name ?? oldDevice.Name,
                GpuModel = cmd.GpuModel ?? oldDevice.GpuModel,
                VramMb = cmd.VramMb ?? oldDevice.VramMb,
                CudaCores = cmd.CudaCores ?? oldDevice.CudaCores,
                DriverVersion = cmd.DriverVersion ?? oldDevice.DriverVersion,
                Frameworks = cmd.Frameworks ?? oldDevice.Frameworks,
                PricePerHourUsdCents = cmd.PricePerHourUsdCents ?? oldDevice.PricePerHourUsdCents,
                OwnerUsername = oldDevice.OwnerUsername,
                State = oldDevice.State,
            };
        }

        public async Task<DeviceAgentInfo> GetAgentInstallInfoAsync(int deviceId)
        {
            var agentInfo = await _api.GetAsync<DeviceAgentInfo>($"/api/devices/{deviceId}/agent-info");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got agent install info for device with ID {id}.", deviceId);
            return agentInfo!;
        }
    }
}
