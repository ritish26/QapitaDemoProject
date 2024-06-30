namespace DemoProject.Services;

public class ServiceManagement : IServiceManagement
{
    public void SendEmail()
    {
        Console.WriteLine($"Send Email: Details are fetched for the database {DateTime.Now:F}");
    }

    public void UpdateDatabase()
    {
        Console.WriteLine($"Update Database: Long Running Task {DateTime.Now:F}");
    }

    public void GenerateMerchandise()
    {
        Console.WriteLine($"Generate Merchandise: Long Running Task {DateTime.Now:F}");
    }

    public void SyncData()
    {
        Console.WriteLine($"Sync Data: Short Running Task {DateTime.Now:F}");
    }
}