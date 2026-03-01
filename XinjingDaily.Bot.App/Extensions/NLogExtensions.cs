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

                const string loggerLayout = "${longdate} [${level:uppercase=true}] ${logger:shortName=true} - ${message} ${exception:format=toString,Data}";

                // ========== 1. 原有彩色控制台目标（通用日志） ==========
                var consoleTarget = new ColoredConsoleTarget("coloredConsole") {
                    Layout = loggerLayout
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

                // ========== 2. 新增sqllog专属控制台目标（特殊颜色） ==========
                var sqlConsoleTarget = new ColoredConsoleTarget("sqlColoredConsole") {
                    Layout = loggerLayout
                };

                // 规则1: SELECT 操作 - 蓝色（ForegroundColor = ConsoleOutputColor.Blue）
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: select', true)",
                    ForegroundColor = ConsoleOutputColor.Blue,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                // 规则2: CREATE 操作 - 绿色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: create', true)",
                    ForegroundColor = ConsoleOutputColor.Green,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                // 规则3: INSERT 操作 - 青色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: insert', true)",
                    ForegroundColor = ConsoleOutputColor.Yellow,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                // 规则4: DELETE 操作 - 红色（醒目提醒删除操作）
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: delete', true)",
                    ForegroundColor = ConsoleOutputColor.Red,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询时间')",
                    ForegroundColor = ConsoleOutputColor.White,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                // 规则5: 其他SQL操作 - 紫色（默认兜底）
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    ForegroundColor = ConsoleOutputColor.Magenta,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                // ========== 注册所有目标到配置 ==========
                config.AddTarget(consoleTarget);
                config.AddTarget(sqlConsoleTarget);

                // ========== 配置日志规则 ==========
                config.AddRule(new LoggingRule("XinjingDaily.Bot.App.Extensions.DatabaseExtension", NLog.LogLevel.Trace, sqlConsoleTarget) {
                    Final = true
                });

                config.AddRule(new LoggingRule("System.Net.Http.*", NLog.LogLevel.Error, consoleTarget) {
                    Final = true
                });

                config.AddRule(new LoggingRule("Microsoft.AspNetCore.Mvc.*", NLog.LogLevel.Error, consoleTarget) {
                    Final = true
                });

                config.AddRuleForAllLevels(consoleTarget);

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