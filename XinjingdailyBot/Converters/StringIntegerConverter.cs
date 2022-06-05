using System.Text.Json;
using System.Text.Json.Serialization;

namespace XinjingdailyBot.Converters
{
    public class StringIntegerConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? "";
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                try
                {
                    return reader.GetInt32().ToString();
                }
                catch
                {
                    return reader.GetSingle().ToString();
                }
            }

            throw new JsonException($"{reader.GetString()} 无法转换为 {typeof(string)}");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
}
