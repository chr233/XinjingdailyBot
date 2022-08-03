using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries.Commands
{
    internal static class SuperCmd
    {
        /// <summary>
        /// 设置用户组
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task SetUserGroup(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            async Task<(string, InlineKeyboardMarkup?)> exec()
            {
                if (args.Length < 3)
                {
                    return ("参数有误", null);
                }

                var targetUser = await FetchUserHelper.FetchDbUserByUserID(args[1]);

                if (targetUser == null)
                {
                    return ("找不到指定用户", null);
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return ("无法对自己进行操作", null);
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return ("无法对同级管理员进行此操作", null);
                }

                if (int.TryParse(args[2], out int groupID))
                {
                    if (UGroups.TryGetValue(groupID, out var group))
                    {
                        targetUser.GroupID = groupID;
                        targetUser.ModifyAt = DateTime.Now;
                        await DB.Updateable(targetUser).UpdateColumns(x => new { x.GroupID, x.ModifyAt }).ExecuteCommandAsync();
                        return ($"修改用户 {targetUser} 权限组成功, 当前权限组 {group.Name}", null);
                    }
                }

                return ($"修改用户 {targetUser} 权限组失败, 找不到指定的权限组", null);
            }

            (string text, var kbd) = await exec();
            await botClient.EditMessageTextAsync(callbackQuery.Message!, text, replyMarkup: kbd);
        }
    }
}
