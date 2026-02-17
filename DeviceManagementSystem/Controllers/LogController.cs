using DeviceManagementAPI.Data;
using DeviceManagementAPI.DTO;
using DeviceManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DeviceManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly IMongoCollection<AuditLog> _auditLogs;
        public LogController(DeviceDbContext context)
        {
            _auditLogs = context.AuditLogs;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] FetchTaskDto dto)
        {
            var tasks = await _auditLogs.Find(t => 
                (dto.DeviceId == Guid.Empty || t.DeviceId == dto.DeviceId) && 
                (string.IsNullOrEmpty(dto.Username) || t.Username == dto.Username) &&
                (string.IsNullOrEmpty(dto.Operation) || t.Operation == dto.Operation)
            )
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
            
            return Ok(tasks);
        }
    }
}