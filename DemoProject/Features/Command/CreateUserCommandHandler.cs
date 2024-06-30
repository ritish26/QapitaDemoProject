using MongoDB.Driver;
using DemoProject.Models;  
using AutoMapper;
using DemoProject.Services;

namespace DemoProject.Features.Command
{
    public class CreateUserCommandHandler : IHandleMessages<CreateUserCommand>
    {

        private readonly IMongoCollection<User> _userCollection;
        private readonly IMapper _mapper;
        private readonly EventStoreHttpClient _eventStoreHttpClient;
        private readonly Serilog.ILogger _logger;
        public CreateUserCommandHandler(IMapper mapper,EventStoreHttpClient eventStoreHttpClient,Serilog.ILogger logger,MongodbService mongoDbService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); 
            _userCollection = mongoDbService.GetUserCollection()?? throw new ArgumentNullException(nameof(_userCollection));
            _eventStoreHttpClient=eventStoreHttpClient ?? throw new ArgumentNullException(nameof(eventStoreHttpClient)); 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task Handle(CreateUserCommand message, IMessageHandlerContext context)
        {
            if (message == null)
            {
                _logger.Information("message cannot be NULL");
                return;
            }
            
            //use auto mapper
            var user =  _mapper.Map<User>(message);
            user.Id = Guid.NewGuid();

            // Push data to EventStore
            await _eventStoreHttpClient.AppendEventAsync(user, "UserCreated");
            _logger.Information("Adding UserCreated {userId} event to EventStore : ", user.Id);
            
            //Read data from EventStore
            await _eventStoreHttpClient.FetchAndWriteEventsToMongoAsync();
            _logger.Information("Data is Inserted for user with Id: {UserId}", user.Id);

        }
    }
}