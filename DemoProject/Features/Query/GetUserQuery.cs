using MediatR;
using DemoProject.Models;

namespace DemoProject.Features.Query;

public class GetUserQuery : IRequest<User>
{
    public GetUserQuery(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    
}
