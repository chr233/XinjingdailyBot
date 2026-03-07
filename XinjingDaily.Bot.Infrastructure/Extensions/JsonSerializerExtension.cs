using System.Text.Json;
using System.Text.Json.Serialization;

namespace XinjingDaily.Bot.Infrastructure.Extensions;

public static class JsonSerializerExtension
{
    // 静态缓存配置，避免重复创建
    private static readonly JsonSerializerOptions _defaultOptions = new JsonSerializerOptions {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    extension<T>(T value)
    {
        /// <summary>
        /// 序列化对象（ASP.NET Core默认规则，生产环境）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">要序列化的对象</param>
        /// <returns>JSON字符串</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(value, _defaultOptions);
        }
        /// <summary>
        /// 序列化为字节数组（UTF8编码）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">要序列化的对象</param>
        /// <returns>UTF8字节数组</returns>
        public byte[] ToJsonBytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _defaultOptions);
        }
    }

    extension(string json)
    {
        /// <summary>
        /// 反序列化JSON字符串（ASP.NET Core默认规则）
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns>反序列化后的对象</returns>
        /// <exception cref="JsonException">JSON格式错误时抛出</exception>
        public T? ToJsonObject<T>() where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(json, _defaultOptions);

        }

        /// <summary>
        /// 安全反序列化（不抛出异常）
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="result">反序列化结果</param>
        /// <returns>是否成功</returns>
        public bool TryToJsonObject<T>(out T? result) where T : class
        {
            result = null;

            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                result = JsonSerializer.Deserialize<T>(json, _defaultOptions);
                return true;
            }
            catch (JsonException ex)
            {
                _logger.Error(ex, "反序列化失败");
                return false;
            }
        }
    }

    extension(byte[] bytes)
    {
        /// <summary>
        /// 从字节数组反序列化
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="bytes">UTF8字节数组</param>
        /// <returns>反序列化后的对象</returns>
        public T? ToJsonObject<T>()
        {
            if (bytes == null || bytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(bytes, _defaultOptions);
        }
    }
}
