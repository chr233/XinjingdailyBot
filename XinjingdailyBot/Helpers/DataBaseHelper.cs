using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal class DataBaseHelper
    {
        public static SqlSugarScope DB;

        internal static void Init()
        {
            Storage.Config config = BotConfig;

            string dbString = $"server={config.DBHost};port={config.DBPort};Database={config.DBName};Uid={config.DBUser};Pwd={config.DBPassword};CharSet=utf8mb4;";

            DB = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = dbString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
            });

            if (config.Debug)
            {
                DB.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Logger.Debug(sql);
                    Logger.Debug(string.Join(",", pars?.Select(it => it.ParameterName + ":" + it.Value)));
                };
            }

            if (config.DBGenerate)
            {
                DB.DbMaintenance.CreateDatabase();

                UpdateTables();
            }
        }

        internal static void UpdateTables()
        {
            DB.CodeFirst.InitTables<Models.Posts>();
            DB.CodeFirst.InitTables<Models.Attachments>();
            DB.CodeFirst.InitTables<Models.Users>();
            DB.CodeFirst.InitTables<Models.Levels>();
            DB.CodeFirst.InitTables<Models.Rights>();
        }


        internal static async Task AddBuildInValues()
        {
            //List


            //DB.Insertable(insertObj).ExecuteCommand();
        }
    }
}
