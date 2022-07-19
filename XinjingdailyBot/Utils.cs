using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using XinjingdailyBot.Storage;

namespace XinjingdailyBot
{
    internal static class Utils
    {
        internal static long BotID { get; set; }
        internal static Config BotConfig => ConfigHelper.BotConfig;

        internal static NLog.Logger Logger => LoggerHelper.Logger;

        internal static SqlSugarScope DB => DataBaseHelper.DB;

        internal static Dictionary<int, Models.Groups> UGroups => DataBaseHelper.UGroups;
        internal static Dictionary<int, Models.Levels> ULevels => DataBaseHelper.ULevels;

        internal static string BotName { get; set; } = "";
        internal static Chat ReviewGroup => ChannelHelper.ReviewGroup;
        internal static Chat CommentGroup => ChannelHelper.CommentGroup;
        internal static Chat SubGroup => ChannelHelper.SubGroup;
        internal static Chat AcceptChannel => ChannelHelper.AcceptChannel;
        internal static Chat RejectChannel => ChannelHelper.RejectChannel;

        internal static Version MyVersion => typeof(Program).Assembly.GetName().Version ?? new Version("0");

        public static bool IsLong(string target)
        {
            try
            {
                long.Parse(target);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// 判断用户是否被封禁
        /// </summary>
        /// <param name="user">目标用户</param>
        /// <returns>如果被封禁，则返回结果为栏目，如果否，则返回null</returns>
        public static async Task<Ban?> IsBan(Users user)
        {
            var dbUser = await DB.Queryable<Ban>().FirstAsync(x => x.UserID == user.UserID);
            if (dbUser != null)
            {
                return dbUser;
            }
            return null;

        }

        /// <summary>
        /// 判断用户是否被封禁
        /// </summary>
        /// <param name="user">目标用户</param>
        /// <returns>如果被封禁，则返回结果为栏目，如果否，则返回null</returns>
        public static async Task<Ban?> IsBan(long userID)
        {
            var dbUser = await DB.Queryable<Ban>().FirstAsync(x => x.UserID == userID);
            if (dbUser != null)
            {
                return dbUser;
            }
            return null;

        }
    }
}
