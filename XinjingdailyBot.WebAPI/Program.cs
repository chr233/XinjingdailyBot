using NLog.Extensions.Logging;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.WebAPI.Extensions;

const string banner = @"
__  _ _             _  _            ___       _  _      
\ \/ <_>._ _  ___  <_><_>._ _  ___ | . \ ___ <_>| | _ _ 
 \ \ | || ' |/ . | | || || ' |/ . || | |<_> || || || | |
_/\_\|_||_|_|\_. | | ||_||_|_|\_. ||___/<___||_||_|`_. |
             <___'<__'        <___'                <___'           
";

Console.WriteLine(Langs.Line);
foreach (var line in banner.Split('\n'))
{
    Console.WriteLine(line);
}
Console.WriteLine(Langs.Line);
Console.WriteLine("框架: {0}", Utils.FrameworkName);
Console.WriteLine("版本: {0} {1} {2}", Utils.Version, Utils.Configuration, BuildInfo.Variant);
Console.WriteLine("作者: {0} {1}", BuildInfo.Author, Utils.Company);
Console.WriteLine("版权: {0}", Utils.Copyright);
Console.WriteLine(Langs.Line);
Console.WriteLine("欢迎使用 XinjingdailyBot");
Console.WriteLine(Langs.Line);
Console.WriteLine("欢迎订阅心惊报 https://t.me/xinjingdaily");
Console.WriteLine(Langs.Line);

Thread.Sleep(2000);

Utils.CleanOldFiles();

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// 配置类支持
services.AddOptions();
services.Configure<OptionsSetting>(builder.Configuration);

// NLog
services.AddLogging(loggingBuilder => {
    loggingBuilder.ClearProviders();
#if !DEBUG
    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
#endif
    loggingBuilder.AddNLog("config/nlog.config");
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
