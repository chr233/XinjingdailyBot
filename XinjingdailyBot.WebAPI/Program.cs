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
Console.WriteLine("源码: {0}", BuildInfo.Repo);
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

// 配置类支持
builder.AddCustomJsonFiles();

// 设置 Kestrel
builder.WebHost.SetupKestrel();

// SqlSugar
services.AddSqlSugarSetup();

// Redis
services.AddRedis();

#if DEBUG
// 添加服务
services.AddAppService();
// 添加定时任务
services.AddQuartzSetup(builder.Configuration);
#else
// 添加服务
services.AddAppServiceGenerated();
// 添加定时任务
services.AddQuartzSetupGenerated(builder.Configuration);
#endif

// 注册HttpClient
services.AddHttpClients();

// Telegram
services.AddTelegramBotClient();

// Web API
services.AddWebAPI(builder.WebHost);

var app = builder.Build();

// Web API
app.UseWebAPI();

app.Run();
