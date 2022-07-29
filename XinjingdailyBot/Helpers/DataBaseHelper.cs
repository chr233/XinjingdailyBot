using SqlSugar;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal class DataBaseHelper
    {
#pragma warning disable CS8618// 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public static SqlSugarScope DB;
#pragma warning restore CS8618// 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        public static Dictionary<int, Groups> UGroups = new();

        public static Dictionary<int, Levels> ULevels = new();

        internal static async Task Init()
        {
            Storage.Config config = BotConfig;

            string dbString = $"Host={config.DBHost};Port={config.DBPort};Database={config.DBName};UserID={config.DBUser};Password={config.DBPassword};CharSet=utf8mb4;AllowZeroDateTime=true";

            DB = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = dbString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
            });

            if (config.Debug)//打印SQL (异步模式貌似没有用)
            {
                DB.Aop.OnLogExecuted = (sql, pars) =>
                {
                    //Logger.Debug(sql);
                    Logger.Debug(UtilMethods.GetSqlString(DbType.SqlServer, sql, pars));
                };
            }

            if (config.DBGenerate)// 重建数据表
            {
                DB.DbMaintenance.CreateDatabase();

                DB.CodeFirst.InitTables<Posts>();
                DB.CodeFirst.InitTables<Attachments>();
                DB.CodeFirst.InitTables<Users>();
                DB.CodeFirst.InitTables<Levels>();
                DB.CodeFirst.InitTables<Groups>();
                DB.CodeFirst.InitTables<Advertises>();
                DB.CodeFirst.InitTables<CmdRecords>();
                DB.CodeFirst.InitTables<BanRecords>();
            }

            await LoadLevelsAndRights();

            if (!ULevels.ContainsKey(1) || !UGroups.ContainsKey(1))
            {
                await AddBuildInValues();
            }
        }

        internal static async Task LoadLevelsAndRights()
        {
            ULevels.Clear();
            List<Levels> levels = await DB.Queryable<Levels>().ToListAsync();
            foreach (var level in levels)
            {
                ULevels.Add(level.Id, level);
            }

            UGroups.Clear();
            List<Groups> rights = await DB.Queryable<Groups>().ToListAsync();
            foreach (var right in rights)
            {
                UGroups.Add(right.Id, right);
            }
        }

        internal static async Task AddBuildInValues()
        {
            List<Levels> levels = new()
            {
                new() { Id = 1, Name = "Lv0", MinExp = 0, MaxExp = 10 },
                new() { Id = 2, Name = "Lv1", MinExp = 11, MaxExp = 100 },
                new() { Id = 3, Name = "Lv2", MinExp = 101, MaxExp = 500 },
                new() { Id = 4, Name = "Lv3", MinExp = 501, MaxExp = 1000 },
                new() { Id = 5, Name = "Lv4", MinExp = 1001, MaxExp = 2000 },
                new() { Id = 6, Name = "Lv5", MinExp = 2001, MaxExp = 5000 },
                new() { Id = 7, Name = "Lv6", MinExp = 5001, MaxExp = 10000 },
                new() { Id = 8, Name = "Lv6+", MinExp = 10001 },
                new() { Id = 9, Name = "Lv-" },
            };

            await DB.Storageable(levels).ExecuteCommandAsync();

            List<Groups> rights = new()
            {
                new() { Id = 1, Name = "普通用户", DefaultRight = UserRights.SendPost | UserRights.NormalCmd },
                new() { Id = 2, Name = "审核员", DefaultRight = UserRights.SendPost | UserRights.ReviewPost | UserRights.NormalCmd },
                new() { Id = 3, Name = "发布员", DefaultRight = UserRights.SendPost | UserRights.DirectPost | UserRights.NormalCmd },
                new() { Id = 4, Name = "狗管理", DefaultRight = UserRights.SendPost | UserRights.ReviewPost | UserRights.DirectPost | UserRights.NormalCmd | UserRights.AdminCmd },
                new() { Id = 5, Name = "超级狗管理", DefaultRight = UserRights.SendPost | UserRights.ReviewPost | UserRights.DirectPost | UserRights.NormalCmd | UserRights.AdminCmd | UserRights.SuperCmd },
                new() { Id = 6, Name = "封禁用户", DefaultRight = UserRights.None },
            };

            await DB.Storageable(rights).ExecuteCommandAsync();

            await LoadLevelsAndRights();
        }
    }
}
