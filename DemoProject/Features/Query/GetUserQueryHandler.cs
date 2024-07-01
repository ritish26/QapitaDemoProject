using MediatR;
using DemoProject.Contracts;
using DemoProject.Models;

namespace DemoProject.Features.Query;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery,User>
{
    private readonly IUserRepository _userRepository;
    private readonly Serilog.ILogger _logger;

    public GetUserQueryHandler(IUserRepository userRepository,Serilog.ILogger logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUser(request.Name, CancellationToken.None);
            if (user != null) return user;
            _logger.Information("User '{UserName}' not found", request.Name);
        }
        
        catch (Exception e)
        {
            _logger.Information("Error getting while retrieving the user details", e.Message);
            throw;
        }

        return null!;
    }
}