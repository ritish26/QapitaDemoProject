using DemoProject.Features.Query;
using DemoProject.Models;

namespace DemoProject.Contracts;

public interface IUserRepository
{
    Task InsertUserAsync(User user, CancellationToken cancellationToken);
    Task<List<User>> GetUserList(GetAllUserQuery request, CancellationToken cancellationToken);
    Task<User>  GetUser(string userId, CancellationToken cancellationToken);
}