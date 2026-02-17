using DeviceManagementAPI.Models;

namespace DeviceManagementAPI.Repositories
{
    public interface IDeviceCache
    {
        public void Add(Device device);
        public bool Modify(Device device);
        public Device? Read(Guid id);
        public void Delete(Guid id);

    }
}