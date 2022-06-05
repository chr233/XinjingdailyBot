using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using XinjingdailyBot.Storage;
using XinjingdailyBot.Localization;
using XinjingdailyBot.Converters;

namespace XinjingdailyBot.Helpers
{
    internal static class ConfigHelper
    {
        internal static Config BotConfig { get; private set; } = new();

        /// <summary>
        /// 读取配置路径
        /// </summary>
        /// <returns></returns>
        public static string GetConfigFilePath()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string cfgPath = Path.Combine(currentDir, Static.ConfigFileName);
            return cfgPath;
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public static void SaveConfig()
        {
            string filePath = GetConfigFilePath();
            SaveConfig(filePath);
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="filePath"></param>
        public static void SaveConfig(string filePath)
        {

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
            };

            string strConfig = JsonSerializer.Serialize(BotConfig, options);

            File.WriteAllText(filePath, strConfig, Encoding.UTF8);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public static async Task LoadConfig()
        {
            string filePath = GetConfigFilePath();

            await LoadConfig(filePath);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="filePath"></param>
        public static async Task LoadConfig(string filePath)
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
