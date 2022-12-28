using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class ReviewCommand
    {
        private readonly ILogger<ReviewCommand> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly LevelRepository _levelRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IChannelService _channelService;
        private readonly IPostService _postService;

        public ReviewCommand(
            ILogger<ReviewCommand> logger,
            ITelegramBotClient botClient,
            IUserService userService,
            LevelRepository levelRepository,
            GroupRepository groupRepository,
            IChannelService channelService,
            IPostService postService)
        {
            _logger = logger;
            _botClient = botClient;
            _userService = userService;
            _levelRepository = levelRepository;
            _groupRepository = groupRepository;
            _channelService = channelService;
            _postService = postService;
        }

        /// <summary>
        /// 自定义拒绝稿件理由
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("NO", UserRights.ReviewPost, Description = "自定义拒绝稿件理由")]
        public async Task ResponseNo(Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                if (message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return "该命令仅限审核群内使用";
                }

                if (message.ReplyToMessage == null)
                {
                    return "请回复审核消息并输入拒绝理由";
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);

                if (post == null)
                {
                    return "未找到稿件";
                }

                var reason = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(reason))
                {
                    return "请输入拒绝理由";
                }

                post.Reason = RejectReason.CustomReason;
                await _postService.RejetPost(post, dbUser, reason);

                return $"已拒绝该稿件, 理由: {reason}";
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false);
        }

        /// <summary>
        /// 修改稿件文字说明
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("EDIT", UserRights.ReviewPost, Description = "修改稿件文字说明")]
        public async Task ResponseEditPost(Message message, string[] args)
        {
            async Task<string> exec()
            {
                if (message.Chat.Type != ChatType.Private && message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return "该命令仅限审核群内使用";
                }

                if (message.ReplyToMessage == null)
                {
                    return "请回复审核消息并输入拒绝理由";
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);
                if (post == null)
                {
                    return "未找到稿件";
                }

                var postUser = await _userService.FetchUserByUserID(post.PosterUID);
                if (postUser == null)
                {
                    return "未找到投稿用户";
                }

                post.Text = string.Join(' ', args).Trim();
                await _postService.Updateable(post).UpdateColumns(x => new { x.Text }).ExecuteCommandAsync();

                return $"稿件描述已更新(投稿预览不会更新)";
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false);
        }
    }
}
