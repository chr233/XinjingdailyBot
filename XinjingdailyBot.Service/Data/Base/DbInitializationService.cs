using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.Service.Data.Base;

/// <summary>
/// 消息接收服务
/// </summary>
/// <remarks>
/// 消息接收服务
/// </remarks>
/// <param name="_logger"></param>
/// <param name="_options"></param>
/// <param name="_dbClient"></param>
public class DbInitializationService(
    ILogger<DbInitializationService> _logger,
    IOptions<OptionsSetting> _options,
    ISqlSugarClient _dbClient) : BackgroundService
{
    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("不兼容剪裁")]
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        //var x = _dbClient.DbMaintenance.GetTableInfoList();

        var config = _options.Value.Database;

        if (config.Generate)
        {
            _logger.LogInformation("开始生成数据库结构");
            //创建数据库
            try
            {
                _dbClient.DbMaintenance.CreateDatabase(config.DbName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "创建数据库失败, 可能没有权限");
            }

            //创建数据表
            var assembly = Assembly.Load("XinjingdailyBot.Model");
            var types = assembly.GetTypes()
                .Where(x => x.GetCustomAttribute<SugarTable>() != null)
                .Where(x => x.GetCustomAttribute<SplitTableAttribute>() == null)
                //.Where(x => x.GetCustomAttribute<ObsoleteAttribute>() == null)
                ;

            foreach (var type in types)
            {
                try
                {
                    _logger.LogInformation("开始创建 {type} 表", type);
                    _dbClient.CodeFirst.InitTables(type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "创建 {type} 表失败", type);
                }
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