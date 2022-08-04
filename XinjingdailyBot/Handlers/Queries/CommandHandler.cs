using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Handlers.Queries.Commands;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Queries
{
    internal class CommandHandler
    {
        /// <summary>
        /// 处理CallbackQuery
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        internal static async Task HandleQuery(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            Message message = callbackQuery.Message!;

            if (args.Length < 2 || !long.TryParse(args[0], out long userID))
            {
                await botClient.AutoReplyAsync("Payload 非法", callbackQuery);
                await botClient.RemoveMessageReplyMarkupAsync(message);
                return;
            }

            //判断消息发起人是不是同一个
            if (dbUser.UserID != userID)
            {
                await botClient.AutoReplyAsync("这不是你的消息, 请不要瞎点", callbackQuery);
                return;
            }

            CmdRecords record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = callbackQuery.Data!,
                Handled = false,
                IsQuery = true,
                ExecuteAt = DateTime.Now,
            };

            try
            {
                (record.Handled, bool autoDelete) = await ExecCommand(botClient, dbUser, callbackQuery, message, args[1..]);

                //定时删除命令消息
                if (autoDelete)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        try
                        {
                            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                        catch
                        {
                            Logger.Error($"删除消息 {message.MessageId} 失败");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                record.Exception = $"{ex.GetType} {ex.Message}";
                record.Error = true;
                throw;
            }
            finally
            {
                await DB.Insertable(record).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message">用户消息原文</param>
        /// <returns>handled,autoDelete</returns>
        private static async Task<(bool, bool)> ExecCommand(ITelegramBotClient botClient, Users dbUser, CallbackQuery callbackQuery, Message message, string[] args)
        {
            bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

            //检查权限
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;

            //是否自动删除消息
            bool autoDelete = true;
            //是否成功响应命令
            bool handled = true;
            switch (args[0].ToUpperInvariant())
            {
                //Common - 通用命令, 不鉴权

                //Normal - 普通命令
                case "SAY" when normal:
                    await NormalCmd.ResponseSay(botClient, callbackQuery, args);
                    break;

                case "CANCEL" when normal:
                    await NormalCmd.ResponseCancel(botClient, callbackQuery, false, args);
                    break;

                case "CANCELCLOSE" when normal:
                case "CANCELANDCLOSE" when normal:
                    await NormalCmd.ResponseCancel(botClient, callbackQuery, true, args);
                    break;

                //Admin - 管理员命令
                case "QUERYUSER" when admin:
                case "SEARCHUSER" when admin:
                    await AdminCmd.ResponseSearchUser(botClient, dbUser, callbackQuery, args);
                    autoDelete = false;
                    break;

                //Super - 超级管理员命令
                case "SETUSERGROUP" when super:
                    await SuperCmd.ResponseSetUserGroup(botClient, dbUser, callbackQuery, args);
                    autoDelete = false;
                    break;

                default:
                    //提示未处理的命令
                    await botClient.AutoReplyAsync("未知命令, 获取帮助 /help", message);
                    handled = false;
                    break;
            }

            //自动删除命令的时机
            //1.autoDelete = true
            //2.在群组中
            //3.成功执行命令
            return (handled, autoDelete && inGroup && handled);
        }
    }
}
