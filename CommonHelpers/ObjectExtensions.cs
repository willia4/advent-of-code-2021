using System.Text.Json;

namespace CommonHelpers;

public static class ObjectExtensions
{
    private static JsonSerializerOptions PrettyPrintJson = new JsonSerializerOptions() { WriteIndented = true }; 
    public static string ToJson<T>(this T o)
    {
        return JsonSerializer.Serialize(o, PrettyPrintJson);
    }
}