using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DemoProject.Models;

namespace DemoProject.Services;

public class MongodbService
{
    private readonly IMongoCollection<User> _userCollection;
    public MongodbService(IOptions<MongodbSettings> mongoDbSettings)
    {
        // Initialize MongoDB client and collection based on settings
        var client = new MongoClient(mongoDbSettings.Value.ConnectionUri);
        var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>(mongoDbSettings.Value.CollectionName);
    }

    public IMongoCollection<User> GetUserCollection()
    {
        // Expose the collection to consumers of this service
        return _userCollection;
    }
}
