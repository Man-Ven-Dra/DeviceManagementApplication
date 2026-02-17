using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeviceManagementAPI.Models
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set;}
        
        public Guid DeviceId {get; set;}
        public string Username {get; set;} = string.Empty;
        public string Operation {get; set;} = string.Empty;
        public string Message {get; set;} = string.Empty;
        public DateTime CreatedAt {get; set;} =DateTime.UtcNow;
    }
}