namespace WebApp.Domain.Extensions;

public static class StringExtensions
{
    public static string GetControllerName(this string fullControllerName)
    {
        return fullControllerName.Substring(0, fullControllerName.IndexOf("Controller"));
    }
}