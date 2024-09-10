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
New
";

Console.WriteLine(Langs.Line);
Console.WriteLine(banner);
Console.WriteLine(Langs.Line);
Console.WriteLine("框架: {0}", BuildInfo.FrameworkName);
Console.WriteLine("版本: {0} {1} {2}", BuildInfo.Version, BuildInfo.Configuration, BuildInfo.Variant);
Console.WriteLine("作者: {0} {1}", BuildInfo.Author, BuildInfo.Company);
Console.WriteLine("版权: {0}", BuildInfo.Copyright);
Console.WriteLine(Langs.Line);
Console.WriteLine("欢迎使用 XinjingdailyBot");
Console.WriteLine(Langs.Line);
Console.WriteLine("欢迎订阅心惊报 https://t.me/xinjingdaily");
Console.WriteLine(Langs.Line);

#if !DEBUG
Thread.Sleep(2000);
#endif

Utils.CleanOldFiles();

var builder = WebApplication.CreateBuilder(args);

// 配置类支持
builder.AddCustomJsonFiles();

// 服务注册
var services = builder.Services;

// NLog
services.AddLogging(loggingBuilder => {
    loggingBuilder.ClearProviders();
#if !DEBUG
    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
#endif
    loggingBuilder.AddNLog("nlog.config");
});

// 设置 Kestrel
builder.WebHost.SetupKestrel();

// SqlSugar
services.AddSqlSugarSetup();

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
