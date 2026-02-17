using BillingMicroservice.Models;
using MongoDB.Driver;

public class BillingDbContext
{
    private readonly IMongoDatabase _database;

    public BillingDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["BillingDatabase:ConnectionString"]);
        _database = client.GetDatabase(configuration["BillingDatabase:DatabaseName"]);
    }

    public IMongoCollection<BillModel> Bills =>
        _database.GetCollection<BillModel>("Bills");
}