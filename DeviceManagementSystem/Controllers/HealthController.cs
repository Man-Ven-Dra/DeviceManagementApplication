using DeviceManagementAPI.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeviceManagementAPI.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IMongoDatabase _database;
        public HealthController(DeviceDbContext context)
        {
            _database = context.Database;
        }

        [HttpGet]
        public async Task<IActionResult> Check()
        {
            try
            {
                await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");

                return Ok(new
                {
                    status = "Healthy!",
                    mongodb = "Connected",
                    time = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new 
                { 
                    status = "Unhealthy", 
                    mongodb = "Disconnected",
                    error = ex.Message 
                });
            }
        }
    }
}