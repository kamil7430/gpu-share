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
    Task<Device> UpdateDeviceAsync(int deviceId, UpdateDeviceRequest cmd);

    /// <summary>
    /// PATCH /api/devices/{id}/availability
    /// Toggles device availability.
    /// </summary>
    Task SetAvailabilityAsync(int deviceId, bool available);

    /// <summary>
    /// DELETE /api/devices/{id}
    /// Removes device from catalog.
    /// </summary>
    Task RemoveDeviceAsync(int deviceId);
}