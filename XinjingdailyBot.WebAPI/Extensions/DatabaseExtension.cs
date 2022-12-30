using System.Reflection;
using SqlSugar;
using SqlSugar.IOC;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions
{
    public static class DatabaseExtension
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static bool IsFirstLoad = true;

        public static void AddSqlSugar(this IServiceCollection services, IConfiguration configuration)
        {
            var debug = configuration.GetValue("Debug", false);
            var dbConfig = configuration.GetSection("Database").Get<OptionsSetting.DatabaseOption>();

            if (dbConfig == null)
            {
                _logger.Error("数据库配置不能为空");
                Environment.Exit(1);
            }

            string connStr = dbConfig.UseMySQL ?
                $"Host={dbConfig.DbHost};Port={dbConfig.DbPort};Database={dbConfig.DbName};UserID={dbConfig.DbUser};Password={dbConfig.DbPassword};CharSet=utf8mb4;AllowZeroDateTime=true" :
                $"DataSource={dbConfig.DbName}.db";

            services.AddSqlSugar(new IocConfig
            {
                ConfigId = 0,
                ConnectionString = connStr,
                DbType = dbConfig.UseMySQL ? IocDbType.MySql : IocDbType.Sqlite,
                IsAutoCloseConnection = true//自动释放
            });

            SugarIocServices.ConfigurationSugar(db =>
            {
                if (debug)
                {
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        var param = db.GetConnectionScope(0).Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                        _logger.Debug("{sql}，{param}", sql, param);
                    };

                    db.Aop.OnError = (e) =>
                    {
                        _logger.Error("执行SQL出错：", e);
                    };
                }

                if (dbConfig.Generate && IsFirstLoad)
                {
                    _logger.Info("开始创建数据库");
                    //创建数据库
                    db.DbMaintenance.CreateDatabase(dbConfig.DbName);

                    //创建数据表
                    Assembly assembly = Assembly.Load("XinjingdailyBot.Model");
                    var types = assembly.GetTypes()
                        .Where(x => x.GetCustomAttribute<SugarTable>() != null);

                    foreach (var type in types)
                    {
                        _logger.Info("开始创建 {type} 表", type);
                        db.CodeFirst.InitTables(type);
                    }
                }

                IsFirstLoad = false;
            });
        }
    }

}
