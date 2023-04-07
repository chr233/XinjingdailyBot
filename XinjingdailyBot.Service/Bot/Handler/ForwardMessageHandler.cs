using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(typeof(IForwardMessageHandler), LifeTime.Singleton)]
    public class ForwardMessageHandler : IForwardMessageHandler
    {
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;

        public ForwardMessageHandler(
            ITelegramBotClient botClient,
            IChannelService channelService)
        {
            _botClient = botClient;
            _channelService = channelService;
        }

        /// <summary>
        /// 处理转发的消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> OnForwardMessageReceived(Users dbUser, Message message)
        {
            var forwardFrom = message.ForwardFrom!;
            var replyMessage = message.ReplyToMessage;
            if (replyMessage != null)
            {
                if (replyMessage.From?.Id == _channelService.BotUser.Id)
                {
                    Random rand = new();

                    //制裁复读机
                    if (message.Text == replyMessage.Text)
                    {
                        if (dbUser.Right.HasFlag(UserRights.AdminCmd) || dbUser.Right.HasFlag(UserRights.SuperCmd))
                        {
                            await _botClient.AutoReplyAsync("原来是狗管理, 惹不起惹不起...", message);
                        }
                        else
                        {
                            int seconds = rand.Next(60, 300);
                            DateTime banTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            var chatId = message.Chat.Id;

                            var sendMsg = await _botClient.AutoReplyAsync($"学我说话很好玩{Emojis.Horse}? 劳资反手就是禁言 <code>{seconds}</code> 秒.", message, ParseMode.Html);
                            try
                            {
                                ChatPermissions permission = new() { CanSendMessages = false, };
                                await _botClient.RestrictChatMemberAsync(chatId, dbUser.UserID, permission, banTime);
                            }
                            catch
                            {
                                await _botClient.DeleteMessageAsync(chatId, sendMsg.MessageId);
                                await _botClient.AutoReplyAsync("原来是狗管理, 惹不起惹不起...", message);
                            }
                        }
                        return false;
                    }

                    if (message.Text == "爬" || message.Text == "爪巴")
                    {
                        if (dbUser.Right.HasFlag(UserRights.AdminCmd) || dbUser.Right.HasFlag(UserRights.SuperCmd))
                        {
                            await _botClient.AutoReplyAsync("嗻", message);
                        }
                    }
                }
            }

            //关键词回复
            string? text = message.Text;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text.Contains("投稿") && dbUser.GroupID == 1)
            {
                await _botClient.AutoReplyAsync("如果想要投稿, 直接将稿件通过私信发给我即可.", message);
            }

            return false;
        }
    }
}
