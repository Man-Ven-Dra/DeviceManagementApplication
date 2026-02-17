using System.Collections.Concurrent;
using DeviceManagementAPI.Data;
using DeviceManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagementAPI.Repositories
{
    [Authorize]
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IMongoCollection<Device> _devices;
        private readonly ConcurrentQueue<AuditLog> _queue;

        private readonly IDeviceCache _cache;
        public DeviceRepository(DeviceDbContext context, ConcurrentQueue<AuditLog> queue, IDeviceCache cache)
        {
            _devices = context.Devices;
            _queue = queue;
            _cache = cache;
        }

        public async Task Add(Device device, string username)
        {
            
            await _devices.InsertOneAsync(device);
            _cache.Add(device);

            _queue.Enqueue(new AuditLog
            {
                DeviceId = device.Id,
                Username = username,
                Operation = "Add",
                Message = $"{username} Added Device: {device.DeviceName}"
            });
        }

        public async Task Delete(Guid id, string username)
        {   
            var device = await _devices.Find(d => d.Id == id).FirstOrDefaultAsync();

            if(device != null){
                _queue.Enqueue(new AuditLog
                {
                    DeviceId = id,
                    Username = username,
                    Operation = "Delete",
                    Message = $"{username} Deleted Device: {device.DeviceName}"
                });

                await _devices.DeleteOneAsync(d => d.Id == id);
                _cache.Delete(id);
            }
        }

        public async Task<IEnumerable<Device>> GetAll()
        {
            foreach (var log in _queue)
            {
                Console.WriteLine(log.DeviceId+" "+log.Message+" "+log.Id);
                
            }
            return await _devices.Find(_ => true).ToListAsync();
        }

        public async Task<Device?> GetById(Guid id, string username)
        {   
            var device = _cache.Read(id);
            if(device==null)
                device = await _devices.Find(d => d.Id == id).FirstOrDefaultAsync();
                if(device!=null)
                    _cache.Add(device);

            if (device != null) 
            {
                _queue.Enqueue(new AuditLog
                {
                    DeviceId = id,
                    Username = username,
                    Operation = "Fetch",
                    Message = $"{username} Fetched Device: {device.DeviceName}"
                });

            }

            return device;
        }

        public async Task Update(Device device, string username)
        {
            var result = await _devices.ReplaceOneAsync(d => d.Id == device.Id, device);

            if (result.MatchedCount == 1)
            {
                _cache.Modify(device);

                _queue.Enqueue(new AuditLog
                {
                    DeviceId = device.Id,
                    Username = username,
                    Operation = "Update",
                    Message = $"{username} Updated Device: {device.DeviceName}"
                });
            }
        }


        public async Task<bool> Exists(Guid id)
        {
            return await _devices.Find(d => d.Id == id).AnyAsync();
        }

        public async Task<PagedResult<Device>> Pagination(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string? sortBy,
            string? sortOrder)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var filter = FilterDefinition<Device>.Empty;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filter = Builders<Device>.Filter.Where(d =>
                    d.MAC.Contains(searchTerm) ||
                    d.IMEI.Contains(searchTerm) ||
                    d.IMSI.Contains(searchTerm) ||
                    d.PlatformType.Contains(searchTerm)
                );
            }

            var sortBuilder = Builders<Device>.Sort;
            var sort = sortOrder?.ToLower() == "desc" 
                ? sortBuilder.Descending(sortBy ?? "Id") 
                : sortBuilder.Ascending(sortBy ?? "Id");

            var totalRecords = (int)await _devices.CountDocumentsAsync(filter);
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var result = await _devices.Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedResult<Device>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                Data = result
            };
        }
    }
}