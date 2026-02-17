using System.Security.Claims;
using DeviceManagementAPI.DTO;
using DeviceManagementAPI.Models;
using DeviceManagementAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class DevicesController : ControllerBase
    {
        private readonly IDeviceRepository _repo;
        private readonly HttpClient _httpClient;
        public DevicesController(IDeviceRepository repo, HttpClient httpClient)
        {
            _repo = repo;
            _httpClient = httpClient;
        }

        private Guid CreateGuidFromString(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            if(device == null)
                return BadRequest();

            var username = User.FindFirstValue(ClaimTypes.Name);

            string combined = device.MAC + device.IMEI + device.IMSI;
            Guid deviceId = CreateGuidFromString(combined);
            device.Id = deviceId;

            var existingDevice = await _repo.GetById(device.Id, username);
            if (existingDevice != null)
                return Conflict("Device already registered");

            device.RegisteredAt = DateTime.UtcNow;
            device.LastUpdatedAt = DateTime.UtcNow;
            device.IsActive = true;

            await _repo.Add(device, username);

            var request = new
            {
                DeviceId = deviceId,
                DeviceName = device.DeviceName,
                ManufactureDate = device.RegisteredAt,
                Price = 100000,
                Tax = 18,
                Discount = 8,
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:5228/api/Bill", request);

            if (response.IsSuccessStatusCode)
            {
                var bill = await response.Content.ReadFromJsonAsync<BillDto>();
                Console.WriteLine(bill);
            }

            return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Device updatedDevice)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);

            var existingDevice = await _repo.GetById(id, username);
            if (existingDevice == null)
                return NotFound();

            if (updatedDevice.Battery < 0 || updatedDevice.Battery > 100)
                return BadRequest("Battery must be between 0 and 100");

            existingDevice.Battery = updatedDevice.Battery;
            existingDevice.IsActive = updatedDevice.IsActive;
            existingDevice.LastUpdatedAt = DateTime.UtcNow;

            await _repo.Update(existingDevice, username);

            return Ok(existingDevice);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devices = await _repo.GetAll();
            return Ok(devices);
        }

        // GET: api/devices/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            
            var username = User.FindFirstValue(ClaimTypes.Name);

            var device = await _repo.GetById(id, username);
            if (device == null) return NotFound();
            return Ok(device);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            
            var username = User.FindFirstValue(ClaimTypes.Name);

            var deletedDevice = await _repo.GetById(id, username);
            if(deletedDevice == null)
                return NotFound();

            await _repo.Delete(id, username);
            return NoContent();
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> Pagination(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            string? sortOrder = "asc"
        )
        {
            var result = await _repo.Pagination(pageNumber, pageSize, searchTerm, sortBy, sortOrder);
            return Ok(result);
        }
    }
}