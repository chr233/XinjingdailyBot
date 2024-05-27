using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions;


/// <summary>
/// 数据库扩展
/// </summary>
public static class DatabaseExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 注册数据库
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    [RequiresUnreferencedCode("不兼容剪裁")]
    public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection("Database").Get<OptionsSetting.DatabaseOption>();

        if (config == null)
        {
            _logger.Error("数据库配置不能为空");
            _logger.Info("按任意键退出...");
            Console.ReadKey();
            Environment.Exit(1);
        }

        var dbType = config.DbType?.ToLowerInvariant() switch {
            "sqlite" => DbType.Sqlite,
            "mysql" => DbType.MySql,
            "postgresql" or
            "pgsql" => DbType.PostgreSQL,
            _ => DbType.Custom,
        };

        if (dbType == DbType.Custom && string.IsNullOrEmpty(config.DbConnectionString))
        {
            _logger.Warn("UseMySQL已弃用, 请使用 DbType 配置数据库类型 MySql, Sqlite, PostgreSql");
#pragma warning disable CS0618 // 类型或成员已过时
            dbType = config.UseMySQL ? DbType.MySql : DbType.Sqlite;
#pragma warning restore CS0618 // 类型或成员已过时
        }

        _logger.Info("数据库驱动: {0}", dbType);

        var connStr = dbType switch {
            DbType.MySql => new MySqlConnectionStringBuilder {
                Server = config.DbHost,
                Port = config.DbPort,
                Database = config.DbName,
                UserID = config.DbUser,
                Password = config.DbPassword,
                CharacterSet = "utf8mb4",
                AllowZeroDateTime = true,
            }.ToString(),
            DbType.Sqlite => new SqliteConnectionStringBuilder {
                DataSource = $"{config.DbName}.db",
            }.ToString(),
            DbType.PostgreSQL => new NpgsqlConnectionStringBuilder {
                Host = config.DbHost,
                Port = (int)config.DbPort,
                Database = config.DbName,
                Username = config.DbUser,
                Password = config.DbPassword,
            }.ToString(),
            DbType.Custom => config.DbConnectionString,
            _ => throw new NotSupportedException("不支持的数据库类型"),
        };

        _logger.Info(connStr);

        services.AddSingleton<ISqlSugarClient>(s => {
            var sqlSugar = new SqlSugarScope(new ConnectionConfig {
                ConnectionString = connStr,
                DbType = dbType,
                IsAutoCloseConnection = true,
            },
            db => {
                if (config.LogSQL)
                {
                    db.Aop.OnLogExecuting = (sql, pars) => {
                        //var param = db.GetConnectionScope(0).Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                        foreach (var par in pars)
                        {
                            sql = sql.Replace(par.ParameterName, par.Value.ToString());
                        }
                        _logger.Debug("执行时间: {time} ms | {sql}", db.Ado.SqlExecutionTime.TotalMilliseconds, sql);
                    };

                    //执行时间超过1秒
                    //if (db.Ado.SqlExecutionTime.TotalSeconds > 1)
                    //{
                    //    //代码CS文件名
                    //    var fileName = db.Ado.SqlStackTrace.FirstFileName;
                    //    //代码行数
                    //    var fileLine = db.Ado.SqlStackTrace.FirstLine;
                    //    //方法名
                    //    var FirstMethodName = db.Ado.SqlStackTrace.FirstMethodName;
                    //    //db.Ado.SqlStackTrace.MyStackTraceList[1].xxx 获取上层方法的信息
                    //}

                    db.Aop.OnError = (e) => _logger.Error("执行SQL出错：", e);
                }
            });

            return sqlSugar;
        });

        if (config.Generate)
        {
#if DEBUG
            services.AddHostedService<DbInitService>();
#else
            services.AddHostedService<GeneratedDbInitService>();
#endif
        }
    }
}
