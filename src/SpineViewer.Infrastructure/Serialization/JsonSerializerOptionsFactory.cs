using System.Text.Json;

namespace SpineViewer.Infrastructure.Serialization;

internal static class JsonSerializerOptionsFactory
{
    public static JsonSerializerOptions CreateDefault()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };
    }
}
