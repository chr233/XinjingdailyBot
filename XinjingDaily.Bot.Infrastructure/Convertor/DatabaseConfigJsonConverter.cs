using System.Text.Json;
using System.Text.Json.Serialization;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.Infrastructure.Convertor;

public sealed class DatabaseConfigJsonConverter : JsonConverter<DatabaseConfig>
{
    public override DatabaseConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<DatabaseConfig>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, DatabaseConfig value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteBoolean(nameof(DatabaseConfig.Generate), value.Generate);
        writer.WriteBoolean(nameof(DatabaseConfig.LogSql), value.LogSql);
        writer.WriteString(nameof(DatabaseConfig.Type), value.Type);
        writer.WriteString(nameof(DatabaseConfig.CustomConnectionString), value.CustomConnectionString);
        writer.WriteString(nameof(DatabaseConfig.Host), value.Host);
        writer.WriteNumber(nameof(DatabaseConfig.Port), value.Port);
        writer.WriteString(nameof(DatabaseConfig.Database), value.Database);
        writer.WriteString(nameof(DatabaseConfig.User), value.User);

        var password = string.IsNullOrEmpty(value.Password) ? null : "***";

        writer.WriteString(nameof(DatabaseConfig.Password), password);
        writer.WriteString(nameof(DatabaseConfig.TablePrefix), value.TablePrefix);

        writer.WriteEndObject();
    }
}
