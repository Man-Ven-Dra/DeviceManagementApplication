namespace BillingMicroservice.Models;

public class BillRequest
{
    public Guid DeviceId {get; set;}
    public string DeviceName {get; set;}
    public decimal Price {get; set;}
    public DateTime ManufactureDate {get; set;}
    public decimal Tax {get; set;}
    public int Discount {get; set;}
}