using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class MockDeviceService : IDeviceService
    {
        public Task<Device> GetDeviceAsync(int deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<DeviceStatus> GetDeviceStatusAsync(int deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<Device> RegisterDeviceAsync(RegisterDeviceRequest cmd)
        {
            throw new NotImplementedException();
        }

        public Task RemoveDeviceAsync(int deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Device>> SearchDevicesAsync(DeviceSearchFilters filters)
        {
            throw new NotImplementedException();
        }

        public Task SetAvailabilityAsync(int deviceId, bool available)
        {
            throw new NotImplementedException();
        }

        public Task<Device> UpdateDeviceAsync(int deviceId, UpdateDeviceRequest cmd)
        {
            throw new NotImplementedException();
        }
    }
}
