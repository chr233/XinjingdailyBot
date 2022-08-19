using System.Text.Encodings.Web;
using System.Text.Json;
using XinjingdailyBot.Converters;
using XinjingdailyBot.Storage;
using static XinjingdailyBot.Utils;
using File = System.IO.File;

namespace XinjingdailyBot.Helpers
{
    internal static class ConfigHelper
    {
        internal static Config BotConfig { get; private set; } = new();

        /// <summary>
        /// 读取配置路径
        /// </summary>
        /// <returns></returns>
        internal static string GetConfigFilePath()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string cfgPath = Path.Combine(currentDir, Static.ConfigFileName);
            return cfgPath;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        internal static void SaveConfig()
        {
            string filePath = GetConfigFilePath();
            SaveConfig(filePath);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="filePath"></param>
        internal static void SaveConfig(string filePath)
        {

            JsonSerializerOptions options = new()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
            };

            string strConfig = JsonSerializer.Serialize(BotConfig, options);

            File.WriteAllText(filePath, strConfig, Encoding.UTF8);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        internal static async Task LoadConfig()
        {
            string filePath = GetConfigFilePath();

            await LoadConfig(filePath);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="filePath"></param>
        internal static async Task LoadConfig(string filePath)
        {
            if (File.Exists(filePath))
            {
                string strConfig = await File.ReadAllTextAsync(filePath, Encoding.UTF8);

                try
                {
                    JsonSerializerOptions JsonOptions = new();
                    JsonOptions.Converters.Add(new StringIntegerConverter());
                    BotConfig = JsonSerializer.Deserialize<Config>(strConfig ?? "", JsonOptions) ?? new();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            else
            {
                BotConfig = new();
                SaveConfig(filePath);
            }
        }
    }
}
