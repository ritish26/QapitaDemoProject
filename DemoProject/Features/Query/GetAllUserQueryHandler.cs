using MediatR;
using DemoProject.Contracts;
using DemoProject.Models;

namespace DemoProject.Features.Query;

public class GetAllUserQueryHandler: IRequestHandler<GetAllUserQuery,List<User>> 
{
    private readonly IUserRepository _userRepository;
    private readonly Serilog.ILogger _logger;
    
    public GetAllUserQueryHandler(IUserRepository userRepository, Serilog.ILogger logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task<List<User>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users= await _userRepository.GetUserList(request, CancellationToken.None);
            if (users != null) 
                return users;
            _logger.Information("Empty list of users");
        }
        
        catch (Exception ex)
        {
            _logger.Information("Error retrieving user details", ex.Message);
        }

        return null!;
    }
}