using SqlSugar;
using SqlSugar.IOC;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions
{

    /// <summary>
    /// 数据库扩展
    /// </summary>
    public static class DatabaseExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static bool IsFirstLoad = true;

        /// <summary>
        /// 注册数据库
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        [RequiresUnreferencedCode("不兼容剪裁")]
        public static void AddSqlSugar(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConfig = configuration.GetSection("Database").Get<OptionsSetting.DatabaseOption>();

            if (dbConfig == null)
            {
                _logger.Error("数据库配置不能为空");
                _logger.Error("按任意键退出...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            _logger.Info("数据库驱动: {0}", dbConfig.UseMySQL ? "MySQL" : "SQLite");

            string connStr = dbConfig.UseMySQL ?
                $"Host={dbConfig.DbHost};Port={dbConfig.DbPort};Database={dbConfig.DbName};UserID={dbConfig.DbUser};Password={dbConfig.DbPassword};CharSet=utf8mb4;AllowZeroDateTime=true" :
                $"DataSource={dbConfig.DbName}.db";

            services.AddSqlSugar(new IocConfig {
                ConfigId = 0,
                ConnectionString = connStr,
                DbType = dbConfig.UseMySQL ? IocDbType.MySql : IocDbType.Sqlite,
                IsAutoCloseConnection = true//自动释放
            });

            SugarIocServices.ConfigurationSugar(db => {
                if (dbConfig.LogSQL)
                {
                    db.Aop.OnLogExecuting = (sql, pars) => {
                        var param = db.GetConnectionScope(0).Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                        _logger.Debug("{sql}，{param}", sql, param);
                    };

                    db.Aop.OnError = (e) => {
                        _logger.Error("执行SQL出错：", e);
                    };
                }

                if (dbConfig.Generate && IsFirstLoad)
                {
                    _logger.Info("开始生成数据库结构");
                    //创建数据库
                    if (!dbConfig.UseMySQL)
                    {
                        db.DbMaintenance.CreateDatabase(dbConfig.DbName);
                    }

                    //创建数据表
                    var assembly = Assembly.Load("XinjingdailyBot.Model");
                    var types = assembly.GetTypes()
                        .Where(x => x.GetCustomAttribute<SugarTable>() != null);

                    foreach (var type in types)
                    {
                        _logger.Info("开始创建 {type} 表", type);
                        db.CodeFirst.InitTables(type);
                    }
                    _logger.Warn("数据库结构生成完毕, 建议禁用 Database.Generate 来加快启动速度");
                }

                IsFirstLoad = false;
            });
        }
    }

}
