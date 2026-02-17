using MongoDB.Driver;
using DeviceManagementAPI.Models;

namespace DeviceManagementAPI.Data
{
   public class DeviceDbContext
    {
        public IMongoDatabase Database { get; }

        public DeviceDbContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDbSettings:ConnectionString"]);

            Database = client.GetDatabase(config["MongoDbSettings:DatabaseName"]);
        }

        public IMongoCollection<Device> Devices => Database.GetCollection<Device>("Devices");
        public IMongoCollection<Admin> Admins => Database.GetCollection<Admin>("Admins");
        public IMongoCollection<AuditLog> AuditLogs => Database.GetCollection<AuditLog>("AuditLogs");
    }
}
