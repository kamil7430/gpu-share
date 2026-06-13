namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IDeviceService
{
    /// <summary>
    /// GET /api/devices
    /// Searches devices using filters and pagination.
    /// Pass <paramref name="anonymous"/> = true to query the public catalog without sending the
    /// caller's Authorization header (e.g. from DevicesPage); the default sends it when logged in.
    /// </summary>
    Task<PagedResult<Device>> SearchDevicesAsync(DeviceSearchFilters filters, bool anonymous = false);

    /// <summary>
    /// GET /api/users/{username}/devices
    /// Returns all devices belonging to user with provided username.
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
}
