namespace WebApp.Domain.Extensions;

public static class StringExtensions
{
    public static string GetControllerName(this string fullControllerName)
    {
        return fullControllerName[..fullControllerName.IndexOf("Controller")];
    }
}