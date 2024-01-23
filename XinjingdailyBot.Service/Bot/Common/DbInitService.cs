using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Reflection;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.Service.Bot.Common;

/// <summary>
/// 消息接收服务
/// </summary>
[Obsolete("使用生成器方法替代")]
public sealed class DbInitService(
        ILogger<DbInitService> _logger,
        IOptions<OptionsSetting> _options,
        ISqlSugarClient _dbClient) : BackgroundService
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dbCotion = _options.Value.Database;
        if (dbCotion.Generate)
        {
            _logger.LogInformation("开始生成数据库结构");
            //创建数据库
            try
            {
                _dbClient.DbMaintenance.CreateDatabase(dbCotion.DbName);
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
