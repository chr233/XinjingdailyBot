using Microsoft.Extensions.Logging;
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
    [AppService(typeof(IGroupMessageHandler), LifeTime.Singleton)]
    internal class GroupMessageHandler : IGroupMessageHandler
    {
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<GroupMessageHandler> _logger;

        public GroupMessageHandler(
            ITelegramBotClient botClient,
            IChannelService channelService,
            ILogger<GroupMessageHandler> logger)
        {
            _botClient = botClient;
            _channelService = channelService;
            _logger = logger;
        }

        public async Task OnGroupTextMessageReceived(Users dbUser, Message message)
        {
            var replyMessage = message.ReplyToMessage;
            if (replyMessage != null)
            {
                if (replyMessage.From?.Id == _channelService.BotUser.Id)
                {
                    Random rand = new();

                    //制裁复读机
                    if (message.Text == replyMessage.Text)
                    {
                        if (dbUser.Right.HasFlag(EUserRights.AdminCmd) || dbUser.Right.HasFlag(EUserRights.SuperCmd))
                        {
                            await _botClient.AutoReplyAsync("原来是狗管理, 惹不起惹不起...", message);
                        }
                        else
                        {
                            int seconds = rand.Next(45, 315);
                            DateTime banTime = DateTime.Now.AddSeconds(seconds);

                            var chatId = message.Chat.Id;
                            try
                            {
                                var permission = new ChatPermissions {
                                    CanSendMessages = false,
                                    CanSendAudios = false,
                                    CanSendDocuments = false,
                                    CanSendPhotos = false,
                                    CanSendVideos = false,
                                    CanSendVideoNotes = false,
                                    CanSendVoiceNotes = false,
                                    CanSendPolls = false,
                                    CanSendOtherMessages = false,
                                };
                                await _botClient.RestrictChatMemberAsync(chatId, dbUser.UserID, permission, untilDate: banTime);
                                var sendMsg = await _botClient.AutoReplyAsync($"学我说话很好玩{Emojis.Horse}? 劳资反手就是禁言 <code>{seconds}</code> 秒.", message, ParseMode.Html);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "禁言失败");
                                await _botClient.AutoReplyAsync("原来是狗管理, 惹不起惹不起...", message);
                            }
                        }
                        return;
                    }

                    if (message.Text == "爬" || message.Text == "爪巴")
                    {
                        if (dbUser.Right.HasFlag(EUserRights.AdminCmd) || dbUser.Right.HasFlag(EUserRights.SuperCmd))
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
                return;
            }

            if (text.Contains("投稿") && dbUser.GroupID == 1)
            {
                await _botClient.AutoReplyAsync("如果想要投稿, 直接将稿件通过私信发给我即可.", message);
            }
        }
    }
}
