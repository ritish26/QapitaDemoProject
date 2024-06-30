using System.Reflection;
using FluentValidation;
using DemoProject.Models;
using DemoProject.Features.Command;
using DemoProject.Services;
using Serilog;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using DemoProject.Contracts;
using DemoProject.DynamicRouting;
using EndpointConfiguration = NServiceBus.EndpointConfiguration;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

//Configure all the services
builder.Services.Configure<MongodbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongodbService>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IServiceManagement, ServiceManagement>();

//configure fluent validation
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
builder.Services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(Program).Assembly));

//configure auto mapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Add Hangfire services.
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDbConnection");

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMongoStorage(mongoConnectionString, "hangfiremongo", new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        },
        Prefix = "hangfire.mongo", 
        CheckConnection = true,
        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
    }));

builder.Services.AddHangfireServer();


//Register Logging Service
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration).WriteTo
        .Seq(builder.Configuration.GetConnectionString("SeqConnectionString") ?? string.Empty));

//configure NService bus
builder.Host.UseNServiceBus((context) =>
{
    var endpointConfiguration = new EndpointConfiguration(builder.Configuration.GetConnectionString("RabbitMqEndpoint"));

    var transport = endpointConfiguration
        .UseTransport<RabbitMQTransport>();
    
    transport.UseConventionalRoutingTopology(QueueType.Quorum);
    
    var connectionString = builder.Configuration.GetConnectionString("RabbitMQTransportConnectionString");
    
    transport.ConnectionString(connectionString);

    // Enable installers only in the development environment
    if (context.HostingEnvironment.IsDevelopment())
    {
        endpointConfiguration.EnableInstallers();
    }
    
    var routing = transport.Routing();

    // Load routes dynamically from assembly
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    foreach (var assembly in assemblies)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var routeToAttribute = type.GetCustomAttribute<RouteToAttribute>();
            if (routeToAttribute != null)
            {
                routing.RouteToEndpoint(type, routeToAttribute.Destination);
            }
        }
    }

    return endpointConfiguration;
});


// Configure EventStoreDB
builder.Services.Configure<EventStoreSettings>(builder.Configuration.GetSection("EventStore"));
builder.Services.AddHttpClient<EventStoreHttpClient>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard();

app.MapHangfireDashboard();

app.Run();
