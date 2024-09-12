using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;
using XinjingdailyBot.Infrastructure.Options;

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
    [RequiresUnreferencedCode("不兼容剪裁")]
    public static void AddSqlSugarSetup(this IServiceCollection services)
    {
        services.AddSingleton<ISqlSugarClient>(s => {
            var config = s.GetRequiredService<IOptions<DatabaseConfig>>().Value;

            var dbType = config.DbType?.ToUpperInvariant() switch {
                "SQLITE" => DbType.Sqlite,
                "MYSQL" => DbType.MySql,
                "POSTGRESQL" or "PGSQL" => DbType.PostgreSQL,
                _ => DbType.Custom,
            };

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

                _ => null,
            };

            if (string.IsNullOrEmpty(connStr))
            {
                _logger.Error("数据库配置有误, 请检查 DbType 和 DbConnectionString");
                _logger.Info("按任意键退出...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(config.DbPassword))
            {
                _logger.Info("数据库连接: {0}", connStr);
            }
            else
            {
                _logger.Info("数据库连接: {0}", connStr.Replace(config.DbPassword, "***"));
            }

            var sqlSugar = new SqlSugarScope(new ConnectionConfig {
                ConnectionString = connStr,
                DbType = dbType,
                IsAutoCloseConnection = true,
            }, db => {
                if (config.LogSQL)
                {
                    db.Aop.OnLogExecuting = (sql, pars) => {
                        _logger.Debug("查询语句: {sql}", sql);

                        if (pars != null && pars.Length > 0)
                        {
                            List<string> values = [];
                            foreach (var par in pars)
                            {
                                values.Add(string.Format("{0} = {1}", par.ParameterName, par.Value ?? "NULL"));
                            }
                            _logger.Debug("查询参数: {values}", string.Join(", ", values));
                        }

                    };

                    db.Aop.OnLogExecuted = (_, _) => _logger.Trace("查询时间 {time} ms ", db.Ado.SqlExecutionTime.TotalMilliseconds);

                    db.Aop.OnError = (e) => _logger.Error("执行SQL出错：", e);
                }
            });

            return sqlSugar;
        });

        services.AddHostedService<DbInitService>();
    }
}
