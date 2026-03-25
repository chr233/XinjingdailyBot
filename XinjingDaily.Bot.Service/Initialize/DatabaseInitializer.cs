using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using XinjingDaily.Bot.Generator;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.InitService;

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
            dbClient.SafeCreateDatabase(_options.Value.Database.Database);

            //创建数据表
            dbClient.CreateTableByTypes(_logger);

            _logger.LogWarning("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
        }

        return Task.CompletedTask;
    }
}