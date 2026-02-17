using System.Text.Json;
using DeviceManagementAPI.Models;
using StackExchange.Redis;

namespace DeviceManagementAPI.Repositories
{
    public class RedisCache : IDeviceCache
    {
        private readonly IDatabase _cache;
        public RedisCache(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        private string GetKey(Guid id)
        {
            return $"device:{id}";
        }
        private string Serialize(Device device)
        {
            return JsonSerializer.Serialize(device);
        }

        private Device? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<Device>(json);
        }

        public void Add(Device device)
        {
            var key = GetKey(device.Id);
            var value = Serialize(device);

            _cache.StringSet(key, value);
        }

        public bool Modify(Device device)
        {
            var key = GetKey(device.Id);
            if(!_cache.KeyExists(key))
                return false;

            var value = Serialize(device);
            _cache.StringSet(key, value);

            return true;
        }

        public Device? Read(Guid id)
        {
            var key = GetKey(id);
            var value = _cache.StringGet(key);

            if (value.IsNullOrEmpty)
                return null;

            return Deserialize(value!);
        }

        public void Delete(Guid id)
        {
            var key = GetKey(id);
            _cache.KeyDelete(key);
        }
    }
}