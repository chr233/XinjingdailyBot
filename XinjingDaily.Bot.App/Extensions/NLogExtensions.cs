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
#if DEBUG
    private const Microsoft.Extensions.Logging.LogLevel MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace;
    private static NLog.LogLevel MinimumNLogLevel = NLog.LogLevel.Trace;
#else
    private const Microsoft.Extensions.Logging.LogLevel MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
    private static NLog.LogLevel MinimumNLogLevel = NLog.LogLevel.Info;
#endif

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
                    loggingBuilder.SetMinimumLevel(MinimumLogLevel);
                    loggingBuilder.AddNLog(path);
                });
            else
            {
                var config = new LoggingConfiguration();

                const string loggerLayout = "${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${logger:shortName=false}|${message} ${exception:format=toString,Data}";

                // ========== 1. 原有彩色控制台目标（通用日志） ==========
                var consoleTarget = new ColoredConsoleTarget("coloredConsole") {
                    Layout = loggerLayout
                };

                // 配置不同日志级别的颜色
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Trace",
                    ForegroundColor = ConsoleOutputColor.Gray,
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Debug",
                    ForegroundColor = ConsoleOutputColor.DarkCyan
                });
                consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "level == LogLevel.Info",
                    ForegroundColor = ConsoleOutputColor.White
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
                    ForegroundColor = ConsoleOutputColor.White,
                    BackgroundColor = ConsoleOutputColor.DarkRed
                });

                // ========== 2. 新增sqllog专属控制台目标（特殊颜色） ==========
                var sqlConsoleTarget = new ColoredConsoleTarget("sqlColoredConsole") {
                    Layout = loggerLayout
                };

                //SELECT 操作 - 青色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: select', true)",
                    ForegroundColor = ConsoleOutputColor.Cyan,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                //CREATE 操作 - 绿色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: create', true)",
                    ForegroundColor = ConsoleOutputColor.Green,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                //INSERT 操作 - 青色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: insert', true)",
                    ForegroundColor = ConsoleOutputColor.Green,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                //UPDATE 操作 - 黄色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: update', true)",
                    ForegroundColor = ConsoleOutputColor.Yellow,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                //DELETE 操作 - 红色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询语句: delete', true)",
                    ForegroundColor = ConsoleOutputColor.Red,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                //查询用时 - 白色
                sqlConsoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule {
                    Condition = "Starts-With(message, '查询时间')",
                    ForegroundColor = ConsoleOutputColor.White,
                    BackgroundColor = ConsoleOutputColor.DarkGray
                });

                //其他操作 - 紫色（默认兜底）
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

                config.AddRule(MinimumNLogLevel, NLog.LogLevel.Fatal, consoleTarget);

                LogManager.GlobalThreshold = MinimumNLogLevel;
                LogManager.Configuration = config;

                services.AddLogging(loggingBuilder => {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(MinimumLogLevel);
                    loggingBuilder.AddNLog(config);
                });
            }
        }
    }
}