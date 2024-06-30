namespace DemoProject.DynamicRouting;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class RouteToAttribute : Attribute
{
    public string Destination { get; }

    public RouteToAttribute(string destination)
    {
        Destination = destination;
    }
}