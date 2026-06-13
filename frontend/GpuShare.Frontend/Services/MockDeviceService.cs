using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using System.Net;

namespace GpuShare.Frontend.Services;

public class MockDeviceService : IDeviceService
{
    public Task<Device> GetDeviceAsync(int deviceId)
    {
        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == deviceId)
            ?? throw new ApiException("Device not found.", HttpStatusCode.NotFound);
        return Task.FromResult(device);
    }

    public Task<DeviceStatus> GetDeviceStatusAsync(int deviceId)
    {
        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == deviceId)
            ?? throw new ApiException("Device not found.", HttpStatusCode.NotFound);

        bool online = device.State != DeviceState.UNAVAILABLE;
        return Task.FromResult(new DeviceStatus
        {
            DeviceId = deviceId,
            Online = online,
            State = device.State,
            UtilizationPercent = device.State == DeviceState.RENTED ? 87.3 : 0,
            MemoryUsedMb = device.State == DeviceState.RENTED ? device.VramMb * 0.72 : 0,
            TemperatureCelsius = device.State == DeviceState.RENTED ? 74 : 42,
            LastHeartbeat = online ? DateTime.UtcNow.AddSeconds(-15) : DateTime.UtcNow.AddMinutes(-10),
        });
    }

    public Task<Device> RegisterDeviceAsync(RegisterDeviceRequest cmd)
    {
        var device = new Device
        {
            DeviceId = MockStore.NextId(),
            OwnerUsername = MockStore.CurrentUser.Username,
            Name = cmd.Name,
            GpuModel = cmd.GpuModel,
            VramMb = cmd.VramMb,
            CudaCores = cmd.CudaCores,
            DriverVersion = cmd.DriverVersion,
            PricePerHourUsdCents = cmd.PricePerHourUsdCents,
            State = DeviceState.UNAVAILABLE,
        };
        MockStore.Devices.Add(device);
        return Task.FromResult(device);
    }

    public Task DeleteDeviceAsync(int deviceId)
    {
        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device != null) MockStore.Devices.Remove(device);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Device>> SearchDevicesAsync(DeviceSearchFilters filters, bool anonymous = false)
    {
        var query = MockStore.Devices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.Name))
            query = query.Where(d => d.Name.Contains(filters.Name, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(filters.GpuModel))
            query = query.Where(d => d.GpuModel.Contains(filters.GpuModel, StringComparison.OrdinalIgnoreCase));
        if (filters.MinVramMb.HasValue)
            query = query.Where(d => d.VramMb >= filters.MinVramMb.Value);
        if (filters.MaxVramMb.HasValue)
            query = query.Where(d => d.VramMb <= filters.MaxVramMb.Value);
        if (filters.MinCudaCores.HasValue)
            query = query.Where(d => d.CudaCores >= filters.MinCudaCores.Value);
        if (filters.MaxCudaCores.HasValue)
            query = query.Where(d => d.CudaCores <= filters.MaxCudaCores.Value);
        if (filters.MinPricePerHourUsdCents.HasValue)
            query = query.Where(d => d.PricePerHourUsdCents >= filters.MinPricePerHourUsdCents.Value);
        if (filters.MaxPricePerHourUsdCents.HasValue)
            query = query.Where(d => d.PricePerHourUsdCents <= filters.MaxPricePerHourUsdCents.Value);
        if (filters.AvailableOnly == true)
            query = query.Where(d => d.State == DeviceState.AVAILABLE);

        var all = query.ToList();
        return Task.FromResult(new PagedResult<Device>
        {
            Items = [.. all.Take(filters.Limit)],
            TotalCount = all.Count,
            PageSize = filters.Limit,
        });
    }

    public Task SetAvailabilityAsync(int deviceId, bool available)
    {
        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
        device?.State = available ? DeviceState.AVAILABLE : DeviceState.UNAVAILABLE;
        return Task.CompletedTask;
    }

    public Task<Device> UpdateDeviceAsync(int deviceId, Device oldDevice, UpdateDeviceRequest cmd)
    {
        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == deviceId)
            ?? throw new ApiException("Device not found.", HttpStatusCode.NotFound);

        if (cmd.Name != null) device.Name = cmd.Name;
        if (cmd.GpuModel != null) device.GpuModel = cmd.GpuModel;
        if (cmd.VramMb.HasValue) device.VramMb = cmd.VramMb.Value;
        if (cmd.CudaCores.HasValue) device.CudaCores = cmd.CudaCores.Value;
        if (cmd.DriverVersion != null) device.DriverVersion = cmd.DriverVersion;
        if (cmd.PricePerHourUsdCents.HasValue) device.PricePerHourUsdCents = cmd.PricePerHourUsdCents.Value;
        if (cmd.State.HasValue) device.State = cmd.State.Value;

        return Task.FromResult(device);
    }
}
