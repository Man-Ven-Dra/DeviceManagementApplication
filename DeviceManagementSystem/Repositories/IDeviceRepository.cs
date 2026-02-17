using DeviceManagementAPI.Models;

namespace DeviceManagementAPI.Repositories
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> GetAll();
        Task<Device?> GetById(Guid id, string username);
        Task Add(Device device, string username);
        Task Update(Device device, string username);
        Task Delete(Guid id, string username);
        Task<bool> Exists(Guid id);
        Task<PagedResult<Device>> Pagination(int pageNumber, int pageSize, string? searchTerm, string? sortBy, string? sortOrder);

    }
}