﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Localization;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages
{
    internal static class GroupHandler
    {
        private static Dictionary<string, string> OneToOneWords { get; } = new()
        {
            { "投稿", "如果想要投稿, 直接将稿件通过私信发给我即可." },
        };

        private static Dictionary<string, List<string>> OneToManyWords { get; } = new()
        {
            { "丁真", new() { "一眼丁真, 鉴定为纯纯的RBQ" , "到达美丽世界最高层理塘", "给大哥递烟" } },
            { "GFW", new() { "社会信用-10", "社会信用-10", "社会信用-20", "社会信用-30", "社会信用-114514" } },
            { "24岁", new() { "事先辈", "要素察觉", "只有冰红茶可以吗", "事学生", "冬之花(意味深)" } },
            { "女装", new() { "事先辈", "要素察觉", "只有冰红茶可以吗", "事学生", "冬之花(意味深)" } },
        };

        internal static async Task HandlerGroupMessage(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            string? text = message.Text;

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            foreach (var item in OneToOneWords)
            {
                if (text.Contains(item.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    var chatId = message.Chat.Id;
                    if (message.ReplyToMessage?.From?.Username == BotName && message.ReplyToMessage?.Text != null && message.ReplyToMessage.Text.Contains(item.Key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (dbUser.Right.HasFlag(UserRights.AdminCmd) || dbUser.Right.HasFlag(UserRights.SuperCmd))
                        {
                            await botClient.SendTextMessageAsync(chatId, "原来是狗管理, 惹不起惹不起...", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                        }
                        else
                        {
                            Random rand = new();
                            int seconds = rand.Next(60, 300);
                            DateTime banTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            var msg = await botClient.SendTextMessageAsync(chatId, $"学我说话很好玩{Emojis.Horse}? 劳资反手就是禁言 <code>{seconds}</code> 秒.", ParseMode.Html, replyToMessageId: message.MessageId, allowSendingWithoutReply: true);
                            try
                            {
                                ChatPermissions permission = new() { CanSendMessages = false, };
                                await botClient.RestrictChatMemberAsync(chatId, dbUser.UserID, permission, banTime);
                            }
                            catch
                            {
                                await botClient.DeleteMessageAsync(chatId, msg.MessageId);
                                await botClient.SendTextMessageAsync(chatId, "原来是狗管理, 惹不起惹不起...", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);
                            }
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, item.Value, ParseMode.Html, replyToMessageId: message.MessageId, allowSendingWithoutReply: true);
                    }
                    return;
                }
            }

            foreach (var item in OneToManyWords)
            {
                if (text.Contains(item.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    Random rd = new();
                    if (rd.Next(100) >= 0)
                    {
                        if (item.Value.Any())
                        {
                            int index = rd.Next(item.Value.Count);
                            await botClient.SendTextMessageAsync(message.Chat.Id, item.Value[index], ParseMode.Html, replyToMessageId: message.MessageId, allowSendingWithoutReply: true);
                            return;
                        }
                    }
                }
            }
        }
    }
}
