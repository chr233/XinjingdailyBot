using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.Service.Bot.Common;

/// <summary>
/// 消息接收服务
/// </summary>
public sealed class DbInitService : BackgroundService
{
    private readonly ILogger<DbInitService> _logger;
    private readonly OptionsSetting.DatabaseOption _option;
    private readonly ISqlSugarClient _dbClient;

    /// <summary>
    /// 消息接收服务
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    /// <param name="dbClient"></param>
    public DbInitService(
        ILogger<DbInitService> logger,
        IOptions<OptionsSetting> options,
        ISqlSugarClient dbClient)
    {
        _logger = logger;
        _option = options.Value.Database;
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
                .Where(x => x.GetCustomAttribute<SugarTable>() != null);

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
