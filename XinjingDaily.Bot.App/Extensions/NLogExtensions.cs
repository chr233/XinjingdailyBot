using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.App.Extensions;

/// <summary>
/// 动态注册服务扩展
/// </summary>
public static class NLogExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册引用程序域中所有有AppService标记的类的服务
        /// </summary>
        /// <param name="services"></param>
        public void AddNLogEx()
        {
            var path = Path.Combine(BuildInfo.AppDir, "nlog.config");
            if (File.Exists(path))
                services.AddLogging(loggingBuilder => {
                    loggingBuilder.ClearProviders();
#if !DEBUG
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
#endif
                    loggingBuilder.AddNLog(path);
                });
            else
            {
                var config = new LoggingConfiguration();

                // 创建彩色控制台目标
                var consoleTarget = new ColoredConsoleTarget("coloredConsole") {
                    Layout = "${level:format=FirstCharacter} ${time} [${logger:shortName=false}] ${message} ${exception:format=toString,Data}"
                };

                // 配置不同日志级别的颜色
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Trace",
                    ForegroundColor = ConsoleOutputColor.DarkGray
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Debug",
                    ForegroundColor = ConsoleOutputColor.White
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Info",
                    ForegroundColor = ConsoleOutputColor.Cyan
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Warn",
                    ForegroundColor = ConsoleOutputColor.Yellow
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Error",
                    ForegroundColor = ConsoleOutputColor.Red
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Fatal",
                    ForegroundColor = ConsoleOutputColor.Red,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                config.AddTarget(consoleTarget);
                config.AddRuleForAllLevels(consoleTarget);

                // 添加过滤器以排除指定的 logger
                config.LoggingRules.Add(new LoggingRule("System.Net.Http.*", NLog.LogLevel.Error, consoleTarget));
                config.LoggingRules.Add(new LoggingRule("Microsoft.AspNetCore.Mvc.*", NLog.LogLevel.Error, consoleTarget));

                LogManager.Configuration = config;

                services.AddLogging(loggingBuilder => {
                    loggingBuilder.ClearProviders();
#if !DEBUG
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
#endif
                    loggingBuilder.AddNLog(config);
                });
            }
        }
    }
}