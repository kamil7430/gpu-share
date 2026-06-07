namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IDeviceService
{
    /// <summary>
    /// GET /api/devices
    /// Searches devices using filters and pagination.
    /// </summary>
    Task<PagedResult<Device>> SearchDevicesAsync(DeviceSearchFilters filters);

    /// <summary>
    /// GET /api/devices/{id}
    /// Returns device details.
    /// </summary>
    Task<Device> GetDeviceAsync(int deviceId);

    /// <summary>
    /// GET /api/devices/{id}/status
    /// Returns current device status and telemetry snapshot.
    /// </summary>
    Task<DeviceStatus> GetDeviceStatusAsync(int deviceId);

    /// <summary>
    /// POST /api/devices
    /// Registers a new GPU device.
    /// </summary>
    Task<Device> RegisterDeviceAsync(RegisterDeviceRequest cmd);

    /// <summary>
    /// PATCH /api/devices/{id}
    /// Updates device configuration and pricing.
    /// </summary>
    Task<Device> UpdateDeviceAsync(int deviceId, Device oldDevice, UpdateDeviceRequest cmd);

    /// <summary>
    /// DELETE /api/devices/{id}
    /// Removes device from catalog.
    /// </summary>
    Task DeleteDeviceAsync(int deviceId);

    /// <summary>
    /// GET /api/devices/{id}/agent-info
    /// Fetches information needed for installing and configuring the device agent on the owner's machine. 
    /// This includes installation instructions, configuration parameters, and any necessary credentials 
    /// or tokens. The frontend can use this information to guide the user through the agent setup process, 
    /// ensuring that the device is properly connected to the GpuShare platform for monitoring and management.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    Task<DeviceAgentInfo> GetAgentInstallInfoAsync(int deviceId);
}