using System.Text.Json;
using System.Text.Json.Serialization;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.Infrastructure.Convertor;

public sealed class RedisConfigJsonConverter : JsonConverter<RedisConfig>
{
    public override RedisConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<RedisConfig>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, RedisConfig value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(RedisConfig.Host), value.Host);
        writer.WriteNumber(nameof(RedisConfig.Port), value.Port);

        var password = string.IsNullOrEmpty(value.Password) ? null : "***";
        writer.WriteString(nameof(RedisConfig.Password), password);

        writer.WriteNumber(nameof(RedisConfig.DefaultDatabase), value.DefaultDatabase);
        writer.WriteBoolean(nameof(RedisConfig.Ssl), value.Ssl);
        writer.WriteString(nameof(RedisConfig.SslHost), value.SslHost);
        writer.WriteString(nameof(RedisConfig.KeyPrefix), value.KeyPrefix);
        writer.WriteNumber(nameof(RedisConfig.ConnectTimeout), value.ConnectTimeout);
        writer.WriteNumber(nameof(RedisConfig.SyncTimeout), value.SyncTimeout);
        writer.WriteNumber(nameof(RedisConfig.AsyncTimeout), value.AsyncTimeout);
        writer.WriteNumber(nameof(RedisConfig.ConnectRetry), value.ConnectRetry);
        writer.WriteNumber(nameof(RedisConfig.KeepAlive), value.KeepAlive);

        writer.WriteEndObject();
    }
}
