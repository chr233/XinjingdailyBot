using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingdailyBot.Infrastructure.Options;

namespace XinjingdailyBot.Service.HostedService;

/// <summary>
/// 消息接收服务
/// </summary>
public class DbInitializationService : BackgroundService
{
    private readonly ILogger<DbInitializationService> _logger;
    private readonly DatabaseConfig _option;
    private readonly ISqlSugarClient _dbClient;

    /// <summary>
    /// 消息接收服务
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="dbClient"></param>
    public DbInitializationService(
        ILogger<DbInitializationService> logger,
        IOptions<DatabaseConfig> options,
        ISqlSugarClient dbClient)
    {
        _logger = logger;
        _option = options.Value;
        _dbClient = dbClient;
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("不兼容剪裁")]
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_option.Generate)
        {
            _logger.LogInformation("开始生成数据库结构");
            //创建数据库
            try
            {
                _dbClient.DbMaintenance.CreateDatabase(_option.DbName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "创建数据库失败, 可能没有权限");
            }

            //创建数据表
            var assembly = Assembly.Load("XinjingdailyBot.Model");
            var types = assembly.GetTypes()
                .Where(x => x.GetCustomAttribute<SugarTable>() != null)
                .Where(x => x.GetCustomAttribute<SplitTableAttribute>() == null); ;

            foreach (var type in types)
            {
                _logger.LogInformation("开始创建 {type} 表", type);
                _dbClient.CodeFirst.InitTables(type);
            }
            _logger.LogWarning("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}