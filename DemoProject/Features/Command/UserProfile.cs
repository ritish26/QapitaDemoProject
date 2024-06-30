using AutoMapper;
using DemoProject.Models;

namespace DemoProject.Features.Command;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserCommand, User>().ReverseMap();
    }
    
}