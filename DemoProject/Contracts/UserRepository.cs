using MongoDB.Driver;
using DemoProject.Features.Query;
using DemoProject.Models;
using DemoProject.Services;

namespace DemoProject.Contracts;

public class UserRepository :IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;

    public UserRepository(MongodbService userCollection)
    {
        _userCollection = userCollection.GetUserCollection()?? throw new ArgumentNullException(nameof(userCollection));
    }

    public async Task InsertUserAsync(User userDto, CancellationToken cancellationToken)
    {
        await _userCollection.InsertOneAsync(userDto, cancellationToken: cancellationToken); 
    }

    public Task<List<User>> GetUserList(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        return _userCollection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<User> GetUser(string userName, CancellationToken cancellationToken)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Name, userName);
        var user = await _userCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return user;
    }
}