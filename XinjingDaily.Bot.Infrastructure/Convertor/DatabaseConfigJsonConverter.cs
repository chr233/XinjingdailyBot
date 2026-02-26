using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.Infrastructure.Convertor;

public sealed class DatabaseConfigJsonConverter : JsonConverter<DatabaseConfig>
{
    public override DatabaseConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var safeOptions = CreateOptionsWithoutThis(options);
        return JsonSerializer.Deserialize<DatabaseConfig>(ref reader, safeOptions);
    }

    public override void Write(Utf8JsonWriter writer, DatabaseConfig value, JsonSerializerOptions options)
    {
        var safeOptions = CreateOptionsWithoutThis(options);
        var node = JsonSerializer.SerializeToNode(value, safeOptions) as JsonObject ?? [];

        node[nameof(DatabaseConfig.Password)] = "***";

        node.WriteTo(writer, safeOptions);
    }

    private static JsonSerializerOptions CreateOptionsWithoutThis(JsonSerializerOptions options)
    {
        var safeOptions = new JsonSerializerOptions(options);

        for (var i = safeOptions.Converters.Count - 1; i >= 0; i--)
        {
            if (safeOptions.Converters[i] is DatabaseConfigJsonConverter)
            {
                safeOptions.Converters.RemoveAt(i);
            }
        }

        return safeOptions;
    }
}
