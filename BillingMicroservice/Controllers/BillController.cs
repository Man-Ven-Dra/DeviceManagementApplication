using BillingMicroservice.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BillingMicroservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillController : ControllerBase
{
    
    public readonly IMongoCollection<BillModel> _bills;
    public BillController(BillingDbContext context)
    {
        _bills = context.Bills;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BillRequest request)
    {
        if(request == null)
                return BadRequest();

        var discountAmount = (request.Price * request.Discount) / 100;
        var priceAfterDiscount = request.Price - discountAmount;

        var taxAmount = (priceAfterDiscount * request.Tax) / 100;
        var finalPrice = priceAfterDiscount + taxAmount;

        var bill = new BillModel
        {
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            Price = request.Price,
            Discount = request.Discount,
            Tax = request.Tax,
            ManufactureDate = request.ManufactureDate,

            FinalPrice = finalPrice,
            SellingDate = DateTime.UtcNow
        };

        await _bills.InsertOneAsync(bill);

        return Ok(bill);
    }

    [HttpGet("{deviceId}")]
    public async Task<IActionResult> Get(Guid deviceId)
    {
        var bills = await _bills.Find(b => b.DeviceId == deviceId).ToListAsync();

        if (bills.Count == 0)
        {
            return NotFound("No Bill For this Device Found!");
        }
        
        return Ok(bills);
    }
}