using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
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
    IServiceProvider _serviceProvider) : IServiceInitializer
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

            using var scope = _serviceProvider.CreateScope();
            var dbClient = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

            //创建数据库

            SafeCreateDatabase(dbClient);

            //创建数据表
#if DEBUG
            CreateTableByReflection(dbClient);
#else
            CreateTableByTypes(dbClient);
#endif

            _logger.LogWarning("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
        }

        return Task.CompletedTask;
    }

    [RequiresUnreferencedCode("不兼容剪裁")]
    private void CreateTableByReflection(ISqlSugarClient db)
    {
        //创建数据表
        var assembly = Assembly.Load("XinjingDaily.Bot.Entry");
        var types = assembly.GetTypes()
            .Where(x => x.GetCustomAttribute<SugarTable>() != null);

        foreach (var type in types)
        {
            _logger.LogInformation("开始创建 {type} 表", type);
            SafeCreateTable(db, type);
        }

#if DEBUG
        LogTableInformation(types);
#endif
    }

    private void CreateTableByTypes(ISqlSugarClient db)
    {

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

    private void LogTableInformation(IEnumerable<Type> types)
    {
        var sb = new StringBuilder();
        foreach (var type in types)
        {
            var fullType = type.FullName;
            sb.AppendLine($"SafeCreateTable<{fullType}>(db);");
        }

        _logger.LogTrace("反射扫描到以下需要创建的表");
        _logger.LogTrace(sb.ToString());
    }
}