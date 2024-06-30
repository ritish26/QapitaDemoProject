using MediatR;
using DemoProject.Contracts;
using DemoProject.Models;

namespace DemoProject.Features.Query;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery,User>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUser(request.Name, CancellationToken.None);
        return user;
    }
}