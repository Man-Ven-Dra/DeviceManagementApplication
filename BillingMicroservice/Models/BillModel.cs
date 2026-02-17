namespace BillingMicroservice.Models;

public class BillModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeviceId {get; set;}
    public string DeviceName {get; set;}
    public decimal Price {get; set;}
    public DateTime ManufactureDate {get; set;}
    public DateTime SellingDate {get; set;}
    public decimal Tax {get; set;}
    public decimal Discount {get; set;}
    public decimal FinalPrice {get; set;}
}