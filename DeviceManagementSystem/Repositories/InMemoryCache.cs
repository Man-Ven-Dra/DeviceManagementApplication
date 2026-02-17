using System.Collections.Concurrent;
using DeviceManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagementAPI.Repositories
{
    public class InMemoryCache : IDeviceCache
    {
        private readonly ConcurrentDictionary<Guid, Device> _device;
        public InMemoryCache(ConcurrentDictionary<Guid, Device> device)
        {
            _device = device;
        }

        public void Add(Device device)
        {
            _device.TryAdd(device.Id, device);
        }

        public bool Modify(Device device)
        {
            Device? modifyDevice = _device.GetValueOrDefault(device.Id);

            if (modifyDevice == null) return false;

            bool result = _device.TryUpdate(device.Id, device, modifyDevice);

            return result;
        }

        public Device? Read(Guid id)
        {
            var readDevice = _device.GetValueOrDefault(id);

            return readDevice;
        }

        public void Delete(Guid id)
        {
            _device.TryRemove(id, out var device);
        }
    }
}