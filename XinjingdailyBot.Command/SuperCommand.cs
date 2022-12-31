using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class SuperCommand
    {
        private readonly ILogger<SuperCommand> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IPostService _postService;
        private readonly IChannelOptionService _channelOptionService;
        private readonly IChannelService _channelService;
        private readonly IMarkupHelperService _markupHelperService;

        public SuperCommand(
            ILogger<SuperCommand> logger,
            ITelegramBotClient botClient,
            IPostService postService,
            IChannelOptionService channelOptionService,
            IChannelService channelService,
            IMarkupHelperService markupHelperService)
        {
            _logger = logger;
            _botClient = botClient;
            _postService = postService;
            _channelOptionService = channelOptionService;
            _channelService = channelService;
            _markupHelperService = markupHelperService;
        }

        /// <summary>
        /// 重启机器人
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("RESTART", UserRights.SuperCmd, Description = "重启机器人")]
        public async Task ResponseRestart(Message message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    Process.Start(Environment.ProcessPath!);
                }
                catch (Exception ex)
                {
                    _logger.LogError("遇到错误", ex);
                }

                await Task.Delay(2000);

                Environment.Exit(0);
            });

            var text = "机器人即将重启";
            await _botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 来源频道设置
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("CHANNELOPTION", UserRights.SuperCmd, Description = "来源频道设置")]
        public async Task ResponseChannalOption(Users dbUser, Message message)
        {
            async Task<(string, InlineKeyboardMarkup?)> exec()
            {
                if (message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return ("该命令仅限审核群内使用", null);
                }

                if (message.ReplyToMessage == null)
                {
                    return ("请回复审核消息并输入拒绝理由", null);
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);

                if (post == null)
                {
                    return ("未找到稿件", null);
                }

                if (!post.IsFromChannel)
                {
                    return ("不是来自其他频道的投稿, 无法设置频道选项", null);
                }

                var channel = await _channelOptionService.FetchChannelByTitle(post.ChannelTitle);

                if (channel == null)
                {
                    return ("未找到对应频道", null);
                }

                string option = channel.Option switch
                {
                    ChannelOption.Normal => "1. 不做特殊处理",
                    ChannelOption.PurgeOrigin => "2. 抹除频道来源",
                    ChannelOption.AutoReject => "3. 拒绝此频道的投稿",
                    _ => "未知的值",
                };

                var keyboard = _markupHelperService.SetChannelOptionKeyboard(dbUser, channel.ChannelID);

                return ($"请选择针对来自 {channel.ChannelTitle} 的稿件的处理方式\n当前设置: {option}", keyboard);
            }

            (var text, var kbd) = await exec();
            await _botClient.SendCommandReply(text, message, autoDelete: false, replyMarkup: kbd);
        }

        /// <summary>
        /// 来源频道设置
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("CHANNELOPTION", UserRights.SuperCmd, Description = "来源频道设置")]
        public async Task QResponseChannalOption(CallbackQuery query, string[] args)
        {
            async Task<string> exec()
            {
                if (args.Length < 3)
                {
                    return "参数有误";
                }

                if (!long.TryParse(args[1], out long channelId))
                {
                    return "参数有误";
                }

                ChannelOption? option = args[2] switch
                {
                    "normal" => ChannelOption.Normal,
                    "purgeorigin" => ChannelOption.PurgeOrigin,
                    "autoreject" => ChannelOption.AutoReject,
                    _ => null
                };

                string optionStr = option switch
                {
                    ChannelOption.Normal => "不做特殊处理",
                    ChannelOption.PurgeOrigin => "抹除频道来源",
                    ChannelOption.AutoReject => "拒绝此频道的投稿",
                    _ => "未知的值",
                };

                if (option == null)
                {
                    return $"未知的频道选项 {args[2]}";
                }

                var channel = await _channelOptionService.UpdateChannelOptionById(channelId, option.Value);

                if (channel == null)
                {
                    return $"找不到频道 {channelId}";
                }

                return $"来自 {channel.ChannelTitle} 频道的稿件今后将被 {optionStr}";
            }

            string text = await exec();
            await _botClient.EditMessageTextAsync(query.Message!, text, replyMarkup: null);
        }
    }


}
