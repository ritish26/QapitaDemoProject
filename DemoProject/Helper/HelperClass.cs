namespace DemoProject.Helper;

public abstract class HelperClass
{
    public static string GetCurrentUser()
    {
        return Environment.UserName;
    }
}