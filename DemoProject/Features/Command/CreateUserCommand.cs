using DemoProject.DynamicRouting;

namespace DemoProject.Features.Command
{
    
    [RouteTo("NewRoute")]
    public  class CreateUserCommand : IMessage
    {
        public string Name { get; set; } = null!;
        public string Email {get; set;} = null!;
        public string Password {get; set;} = null!;
    }
}