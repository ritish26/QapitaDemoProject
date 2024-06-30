using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using DemoProject.Contracts;
using DemoProject.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ILogger = Serilog.ILogger;

namespace DemoProject.Services;

public class EventStoreHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly EventStoreSettings _settings;
    private const string StreamName = "RRstream";
    private readonly ILogger? _logger;
    private readonly IUserRepository _userRepository;

    public EventStoreHttpClient(HttpClient httpClient, IOptions<EventStoreSettings> settings, ILogger logger,IUserRepository userRepository)
    {
        // Initialize fields and configure HttpClient
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _userRepository = userRepository;
        
        // Set up basic authentication(every time with request it get authenticated) header for HttpClient with the authentication header
        var authToken = Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
        
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task AppendEventAsync(object eventData, string eventType)
    {
        var eventPayload = new[]
        {
            new
            {
                eventId = Guid.NewGuid(),
                eventType,
                data = eventData
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(eventPayload), Encoding.UTF8,
            "application/vnd.eventstore.events+json"); //Media type for the event store 
        var response = await _httpClient.PostAsync($"{_settings.HttpUrl}/streams/{StreamName}", content);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string?> GetLastNumber()
    {
        var url = $"http://localhost:2120/streams/{StreamName}/head/backward/1";
        
        try
        {
            // Send the GET request
            HttpResponseMessage response3 = await _httpClient.GetAsync(url);

            // Ensure the request was successful
            response3.EnsureSuccessStatusCode();

            // Read the response content as a string
            var responseBody = await response3.Content.ReadAsStringAsync();
            var json1 = JObject.Parse(responseBody);
            if (json1["entries"] is JArray entries1)
            {
                foreach (var entry in entries1)
                {
                    var data = entry["title"].ToString();
                    string?[] parts = data.Split('@');

                    // Assign the parts to variables
                    var numberPart = parts[0];
                    return numberPart;
                }

            }
        }

        catch (HttpRequestException e)
        {
            // Handle any errors
            Console.WriteLine($"Request error: {e.Message}");
        }

        return null;

    }

    public async Task FetchAndWriteEventsToMongoAsync()
    {
        var number = await GetLastNumber();
        
        Debug.Assert(number != null, nameof(number) + " != null");
        var intValue = int.Parse(number)+1;
        
        //sends the get request over httpclient top fetch data from EventStore
        var response = await _httpClient.GetAsync($"{_settings.HttpUrl}/streams/{StreamName}/0/forward/{intValue}?embed=body");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        
        //JObject.Parse() method to convert a JSON string into a JSON object.
        //convert the response to Jobject (key-value pair) Object
        var json = JObject.Parse(responseContent);
        if (json["entries"] is JArray entries)
        {
            foreach (var entry in entries)
            {
                var data = entry["data"]!.ToString(); 
                
                // Parse the 'data' string into a JObject representing the event data
                var eventData = JObject.Parse(data);

                var user = new User
                {
                    Id=(Guid)(eventData["Id"]!),
                    Name = eventData["Name"]!.ToString(),
                    Email = eventData["Email"]!.ToString(),
                    Password = eventData["Password"]!.ToString()
                };
                
                //Insert in mongodb collection
               await _userRepository.InsertUserAsync(user,CancellationToken.None);
               _logger?.Information("UserCreated event processed successfully");
            }
        }
    }
    
}