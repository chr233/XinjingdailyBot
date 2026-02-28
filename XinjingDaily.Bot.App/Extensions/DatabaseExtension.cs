using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using SqlSugar;
using System.Reflection;
using System.Text.Json;
using XinjingDaily.Bot.Entry.Columns;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Convertor;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.App.Extensions;


/// <summary>
/// 数据库扩展
/// </summary>
public static class DatabaseExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 安全输出数据库配置（隐藏密码）
    /// </summary>
    /// <param name="config"></param>
    private static void PrintDatabaseConfig(DatabaseConfig config)
    {
#pragma warning disable CA1869 // 缓存并重用“JsonSerializerOptions”实例
        var options = new JsonSerializerOptions {
            WriteIndented = true,
        };
#pragma warning restore CA1869 // 缓存并重用“JsonSerializerOptions”实例
        options.Converters.Add(new DatabaseConfigJsonConverter());

        var json = JsonSerializer.Serialize(config, options);

        _logger.Warn("当前 Database 配置: {0}", json);
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册数据库
        /// </summary>
        /// <param name="services"></param>
        public void AddSqlSugarSetup()
        {
            services.AddScoped<ISqlSugarClient>(s => {
                var config = s.GetRequiredService<IOptions<AppSettings>>().Value.Database;

                var dbType = config.Type?.ToUpperInvariant() switch {
                    "SQLITE" => DbType.Sqlite,
                    "MYSQL" => DbType.MySql,
                    "POSTGRESQL" or "PGSQL" => DbType.PostgreSQL,
                    _ => DbType.Custom,
                };

                var connStr = dbType switch {
                    DbType.MySql => new MySqlConnectionStringBuilder {
                        Server = config.Host,
                        Port = config.Port,
                        Database = config.Database,
                        UserID = config.User,
                        Password = config.Password,
                        CharacterSet = "utf8mb4",
                        AllowZeroDateTime = true,
                    }.ToString(),

                    DbType.Sqlite => new SqliteConnectionStringBuilder {
                        DataSource = $"{config.Database}.db",
                    }.ToString(),

                    DbType.PostgreSQL => new NpgsqlConnectionStringBuilder {
                        Host = config.Host,
                        Port = (int)config.Port,
                        Database = config.Database,
                        Username = config.User,
                        Password = config.Password,
                    }.ToString(),

                    DbType.Custom => config.CustomConnectionString,

                    _ => null,
                };

                if (string.IsNullOrEmpty(config.Database) || string.IsNullOrEmpty(connStr))
                {
                    PrintDatabaseConfig(config);
                    _logger.Error("数据库配置有误, 请检查 Database 节配置");
                    _logger.Info("按任意键退出...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                _logger.Info("数据库驱动: {0}", dbType);

                if (string.IsNullOrEmpty(config.Password))
                {
                    _logger.Info("数据库连接: {0}", connStr);
                }
                else
                {
                    _logger.Info("数据库连接: {0}", connStr.Replace(config.Password, "***"));
                }

                var sqlSugarConfig = new ConnectionConfig {
                    ConnectionString = connStr,
                    DbType = dbType,
                    IsAutoCloseConnection = true,
                };

                if (!string.IsNullOrEmpty(config.TablePrefix))
                {
                    var externalService = new ConfigureExternalServices {
                        EntityNameService = (type, entity) => {
                            var attribute = type.GetCustomAttribute<SugarTable>(true);
                            var tableName = attribute?.TableName ?? type.Name;
                            entity.DbTableName = $"{config.TablePrefix}_{tableName}";
                        }
                    };

                    sqlSugarConfig.ConfigureExternalServices = externalService;
                }

                var db = new SqlSugarClient(sqlSugarConfig);

                if (config.LogSql)
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

                }

                db.Aop.OnError = (e) => _logger.Error("执行SQL出错：", e);

                // 2. 全局软删除过滤
                db.QueryFilter.AddTableFilter<ISoftDelete>(t => t.IsDeleted == false);

                return db;
            });
        }
    }
}
