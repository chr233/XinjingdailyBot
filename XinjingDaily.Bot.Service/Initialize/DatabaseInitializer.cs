using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.InitService;

#if DEBUG
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#endif

namespace XinjingDaily.Bot.Service.InitService;

/// <summary>
/// 数据库初始化服务
/// </remarks>
/// <param name="_logger"></param>
[RegisterScoped<IServiceInitializer>(Duplicate = DuplicateStrategy.Append, Registration = RegistrationStrategy.ImplementedInterfaces)]
public class DatabaseInitializer(
    ILogger<DatabaseInitializer> _logger,
    IOptions<AppSettings> _options,
    IServiceScopeFactory _serviceScopeFactory) : IServiceInitializer
{
    /// <inheritdoc/>
    public int Order => 1;

    /// <inheritdoc/>

    public Task InitializeAsync()
    {
        var config = _options.Value.Database;

        if (config.Generate)
        {
            _logger.LogInformation("开始生成数据库结构");

            using var scope = _serviceScopeFactory.CreateScope();
            var dbClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

            //创建数据库
            SafeCreateDatabase(dbClient);

            //创建数据表
#if RELEASE
            dbClient.CreateXinjingDailyBotEntryTables(_logger);
#else
            CreateTableByReflection(dbClient, "XinjingDaily.Bot.Entry");
#endif

            _logger.LogWarning("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
        }

        return Task.CompletedTask;
    }

#if DEBUG
    [RequiresUnreferencedCode("不兼容剪裁")]
    private void CreateTableByReflection(ISqlSugarClient db, string table)
    {
        //创建数据表
        var assembly = Assembly.Load("XinjingDaily.Bot.Entry");
        var types = assembly.GetTypes()
            .Where(x => x.GetCustomAttribute<SugarTable>() != null);

        foreach (var type in types)
        {
            SafeCreateTable(db, type);
            _logger.LogDebug("创建表 {type} 成功", type);
        }
    }


    private void SafeCreateTable(ISqlSugarClient db, Type type)
    {
        try
        {
            db.CodeFirst.InitTables(type);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建表 {type} 失败, 可能没有权限", type.Name);
        }
    }
#endif

    private void SafeCreateDatabase(ISqlSugarClient db)
    {
        try
        {
            db.DbMaintenance.CreateDatabase(_options.Value.Database.Database);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建数据库失败, 可能没有权限");
        }
    }
}