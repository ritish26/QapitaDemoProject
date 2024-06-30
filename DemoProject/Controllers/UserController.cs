using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DemoProject.Features.Command;
using DemoProject.Features.Query;
using DemoProject.Helper;
using DemoProject.Models;
using DemoProject.Services;

namespace DemoProject.Controllers
{ 
   
    [ApiController]
    [Route("api/users")]                          
    public class UserController : ControllerBase
    {
        private readonly IMessageSession _messageSession;
        private readonly Serilog.ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IValidator<CreateUserCommand>  _validator;

        public UserController(IMessageSession messageSession,Serilog.ILogger logger,IMediator mediator,IValidator<CreateUserCommand> validator)
        {
            _messageSession = messageSession;
            _logger = logger;
            _mediator = mediator;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateUserCommand createUserCommand) 
        {
            //validation of the user Input (Fluent Validation)
            var validationResult = await _validator.ValidateAsync(createUserCommand);
            if (validationResult.Errors.Any())
            {
                throw new Exception("Invalidation Result, Please check",new Exception(validationResult.ToString()));
            }
            
            var currentUserName= HelperClass.GetCurrentUser();
            await _messageSession.Send(createUserCommand);
            
            _logger.Information("Successfully posted details for  user {username} : ",currentUserName); 
             RecurringJob.AddOrUpdate<IServiceManagement>(x=>x.UpdateDatabase(), Cron.Minutely);

             return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> Get()
        {
                var query = new GetAllUserQuery();
                var users= await _mediator.Send(query);
                var currentUserName= HelperClass.GetCurrentUser();
                _logger.Information("Fetching all users details {currentUserName} : ", currentUserName);
                BackgroundJob.Enqueue<IServiceManagement>(x => x.SendEmail());
                if (users.Count != 0) return Ok(users);
                
                _logger.Information("The user list is empty");
                return NotFound();

        }
        
        [HttpGet("{userName}")]
        public async Task<ActionResult<User>> GetThroughId(string userName)
        {
            var query = new GetUserQuery(userName);
            var user = await _mediator.Send(query);
           
            var currentUserName= HelperClass.GetCurrentUser();
            if (user != null)
            {
                BackgroundJob.Enqueue<IServiceManagement>(x => x.SendEmail());
                _logger.Information("Fetching the user details for {userName}", userName);
                return Ok(user); 
            }

            _logger.Information("No such user {currentUserName} is present", currentUserName);
            return NotFound();
        }
    }
}