using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthChecks.UI;

public static class JsonSeraliserSettings
{
    public static JsonSerializerOptions Options { get; set; }

    static JsonSeraliserSettings()
    {
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            AllowTrailingCommas = true
        };
        opts.Converters.Add(new JsonStringEnumConverter());

        Options = opts;
    }
}
