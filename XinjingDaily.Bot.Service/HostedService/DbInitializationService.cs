using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.Service.HostedService;

/// <summary>
/// 消息接收服务
/// </summary>
/// <remarks>
/// 消息接收服务
/// </remarks>
/// <param name="_logger"></param>
/// <param name="options"></param>
/// <param name="dbClient"></param>
public class DbInitializationService(
    ILogger<DbInitializationService> _logger,
    IOptions<OptionSettings> _options,
    IServiceProvider _serviceProvider) : BackgroundService
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("不兼容剪裁")]
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var config = _options.Value.Database;

        if (config.Generate)
        {
            _logger.LogInformation("开始生成数据库结构");

            using var scope = _serviceProvider.CreateScope();
            var dbClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

            //创建数据库

            SafeCreateDatabase(dbClient);

            //创建数据表
            var assembly = Assembly.Load("XinjingDaily.Bot.Entry");
            var types = assembly.GetTypes()
                .Where(x => x.GetCustomAttribute<SugarTable>() != null)
                //.Where(x => x.GetCustomAttribute<ObsoleteAttribute>() == null)
                ;

            foreach (var type in types)
            {
                _logger.LogInformation("开始创建 {type} 表", type);
                SafeCreateTable(dbClient, type);
            }

            _logger.LogWarning("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
        }

        return Task.CompletedTask;
    }

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

    private void SafeCreateTable<T>(ISqlSugarClient db)
    {
        try
        {
            db.CodeFirst.InitTables<T>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建表失败, 可能没有权限");
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
            _logger.LogWarning(ex, "创建表失败, 可能没有权限");
        }
    }
}