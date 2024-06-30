using MediatR;
using DemoProject.Contracts;
using DemoProject.Models;

namespace DemoProject.Features.Query;

public class GetAllUserQueryHandler: IRequestHandler<GetAllUserQuery,List<User>> 
{
    private readonly IUserRepository _userRepository;
    
    public GetAllUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;

    }
    
    public async Task<List<User>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
    {
        return await _userRepository.GetUserList(request, CancellationToken.None);
    }
}