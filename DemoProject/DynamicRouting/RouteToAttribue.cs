namespace DemoProject.DynamicRouting;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RouteToAttribute : Attribute //We cannot drive the child class from this class
{
    public string Destination { get; }

    public RouteToAttribute(string destination)
    {
        Destination = destination;
    }
}