using NLog.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.WebAPI.Extensions;

namespace XinjingdailyBot.WebAPI;

/// <summary>
/// 根程序集
/// </summary>
public static class Program
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 启动入口
    /// </summary>
    /// <param name="args"></param>
    [RequiresUnreferencedCode("不兼容剪裁")]
    public static void Main(string[] args)
    {
        const string banner = @"
__  _ _             _  _            ___       _  _      
\ \/ <_>._ _  ___  <_><_>._ _  ___ | . \ ___ <_>| | _ _ 
 \ \ | || ' |/ . | | || || ' |/ . || | |<_> || || || | |
_/\_\|_||_|_|\_. | | ||_||_|_|\_. ||___/<___||_||_|`_. |
             <___'<__'        <___'                <___'           
";

        _logger.Info(Langs.Line);
        foreach (var line in banner.Split('\n'))
        {
            _logger.Info(line);
        }
        _logger.Info(Langs.Line);
        _logger.Info("框架: {0}", Utils.FrameworkName);
        _logger.Info("版本: {0} {1} {2}", Utils.Version, Utils.Configuration, BuildInfo.Variant);
        _logger.Info("作者: {0} {1}", BuildInfo.Author, Utils.Company);
        _logger.Info("版权: {0}", Utils.Copyright);
        _logger.Info(Langs.Line);
        _logger.Info("欢迎使用 XinjingdailyBot");
        _logger.Info(Langs.Line);
        _logger.Warn("欢迎订阅心惊报 https://t.me/xinjingdaily");
        _logger.Info(Langs.Line);

        Thread.Sleep(2000);

        CleanOldFiles();

        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // 配置类支持
        services.AddOptions();
        services.Configure<OptionsSetting>(builder.Configuration);

        // NLog
        services.AddLogging(loggingBuilder => {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            loggingBuilder.AddNLog();
        });

        // SqlSugar
        services.AddSqlSugarSetup(builder.Configuration);

        // 添加服务
#if DEBUG
        services.AddAppService();
#else
        services.AddAppServiceGenerated();
#endif

        // 注册HttpClient
        services.AddHttpClients();

        // Telegram
        services.AddTelegramBotClient();

        // 添加定时任务
#if DEBUG
        services.AddQuartzSetup(builder.Configuration);
#else
        services.AddQuartzSetupGenerated(builder.Configuration);
#endif

        // Web API
        services.AddWebAPI(builder.WebHost);

        var app = builder.Build();

        // Web API
        app.UseWebAPI();

        app.Run();
    }

    /// <summary>
    /// 清除升级文件
    /// </summary>
    private static void CleanOldFiles()
    {
        var bakFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.bak");
        foreach (var bakPath in bakFiles)
        {
            try
            {
                File.Delete(bakPath);
                _logger.Warn("清理升级残留文件");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "清理升级残留文件失败");
            }
        }
    }
}
