namespace WebApp.Domain.Extensions;

public static class TypeExtensions
{
    /// <summary>
    /// Convert a C# type to an HTML input type
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns> <summary>
    public static string ConvertToHtmlInputType(this Type dataType)
    {
        if (dataType == typeof(string))
        {
            return "text";
        }
        else if (dataType == typeof(int) || dataType == typeof(decimal) || dataType == typeof(double))
        {
            return "number";
        }
        else if (dataType == typeof(bool))
        {
            return "checkbox";
        }
        else if (dataType == typeof(DateTime))
        {
            return "date";
        }
        else if (dataType == typeof(TimeSpan))
        {
            return "time";
        }
        // Add more type mappings as needed
        else
        {
            // Default to text if no mapping is found
            return "text";
        }
    }
}