using System.Text;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Command
{
    [AppService(LifeTime.Scoped)]
    public class NormalCommand
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly LevelRepository _levelRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IAttachmentService _attachmentService;
        private readonly IPostService _postService;

        public NormalCommand(
            ITelegramBotClient botClient,
            IUserService userService,
            LevelRepository levelRepository,
            GroupRepository groupRepository,
            IMarkupHelperService markupHelperService,
            IAttachmentService attachmentService,
            IPostService postService)
        {
            _botClient = botClient;
            _userService = userService;
            _levelRepository = levelRepository;
            _groupRepository = groupRepository;
            _markupHelperService = markupHelperService;
            _attachmentService = attachmentService;
            _postService = postService;
        }

        /// <summary>
        /// 检测机器人是否存活
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("PING", UserRights.NormalCmd, Description = "检测机器人是否存活")]
        public async Task ResponsePing(Message message)
        {
            await _botClient.SendCommandReply("PONG!", message);
        }

        /// <summary>
        /// 设置是否匿名
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("ANONYMOUS", UserRights.NormalCmd, Alias = "ANYMOUSE", Description = "设置是否匿名")]
        public async Task ResponseAnonymous(Users dbUser, Message message)
        {
            if (message.Chat.Type != ChatType.Private)
            {
                await _botClient.SendCommandReply("仅能在私聊中使用", message);
                return;
            }

            bool anymouse = !dbUser.PreferAnonymous;
            dbUser.PreferAnonymous = anymouse;
            dbUser.ModifyAt = DateTime.Now;
            await _userService.Updateable(dbUser).UpdateColumns(x => new { x.PreferAnonymous, x.ModifyAt }).ExecuteCommandAsync();

            var mode = anymouse ? "匿名投稿" : "保留来源";
            var text = $"后续投稿将默认使用【{mode}】";
            await _botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 设置稿件审核后是否通知
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("NOTIFICATION", UserRights.NormalCmd, Description = "设置稿件审核后是否通知")]
        public async Task ResponseNotification(Users dbUser, Message message)
        {
            if (message.Chat.Type != ChatType.Private)
            {
                await _botClient.SendCommandReply("仅能在私聊中使用", message);
                return;
            }

            bool notificationg = !dbUser.Notification;
            dbUser.Notification = notificationg;
            dbUser.ModifyAt = DateTime.Now;
            await _userService.Updateable(dbUser).UpdateColumns(x => new { x.Notification, x.ModifyAt }).ExecuteCommandAsync();

            var mode = notificationg ? "接收通知" : "静默模式";
            var text = $"稿件被审核或者过期时将会尝试通知用户\n当前通知设置: {mode}";
            await _botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 获取自己的信息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("MYINFO", UserRights.NormalCmd, Description = "获取自己的信息")]
        public async Task ResponseMyInfo(Users dbUser, Message message)
        {
            StringBuilder sb = new();

            sb.AppendLine("-- 基础信息 --");
            sb.AppendLine(_userService.GetUserBasicInfo(dbUser));

            sb.AppendLine();
            sb.AppendLine("-- 用户排名 --");
            sb.AppendLine(await _userService.GetUserRank(dbUser));

            await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }


        /// <summary>
        /// 获取自己的权限
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("MYRIGHT", UserRights.NormalCmd, Description = "获取自己的权限")]
        public async Task ResponseMyRight(Users dbUser, Message message)
        {
            var right = dbUser.Right;
            bool superCmd = right.HasFlag(UserRights.SuperCmd);
            bool adminCmd = right.HasFlag(UserRights.AdminCmd);
            bool normalCmd = right.HasFlag(UserRights.NormalCmd);
            bool sendPost = right.HasFlag(UserRights.SendPost);
            bool reviewPost = right.HasFlag(UserRights.ReviewPost);
            bool directPost = right.HasFlag(UserRights.DirectPost);
            string userNick = message.From!.EscapedNickName();

            string group = _groupRepository.GetGroupName(dbUser.GroupID);

            List<string> functions = new();
            if (sendPost) { functions.Add("投递稿件"); }
            if (reviewPost) { functions.Add("审核稿件"); }
            if (directPost) { functions.Add("直接发布稿件"); }
            if (functions.Count == 0) { functions.Add("无"); }

            List<string> commands = new();
            if (superCmd) { commands.Add("所有命令"); }
            if (adminCmd) { commands.Add("管理员命令"); }
            if (normalCmd) { commands.Add("普通命令"); }
            if (functions.Count == 0) { commands.Add("无"); }

            StringBuilder sb = new();
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"功能: <code>{string.Join(", ", functions)}</code>");
            sb.AppendLine($"命令: <code>{string.Join(", ", commands)}</code>");

            await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 艾特群管理
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("ADMIN", UserRights.NormalCmd, Description = "艾特群管理")]
        public async Task ResponseCallAdmins(Message message)
        {
            StringBuilder sb = new();

            if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
            {
                sb.AppendLine("该命令仅在群组内有效");
            }
            else
            {
                var admins = await _botClient.GetChatAdministratorsAsync(message.Chat.Id);

                foreach (var menber in admins)
                {
                    var admin = menber.User;
                    if (!(admin.IsBot || string.IsNullOrEmpty(admin.Username)))
                    {
                        sb.AppendLine($"@{admin.Username}");
                    }
                }
            }

            await _botClient.SendCommandReply(sb.ToString(), message);
        }

        /// <summary>
        /// 取消命令
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("CANCEL", UserRights.NormalCmd, Alias = "CANCELCLOSE CANCELANDCLOSE")]
        public async Task QResponseCancel(CallbackQuery query, string[] args)
        {
            string text = args.Length > 1 ? string.Join(' ', args[1..]) : "操作已取消";

            await _botClient.AutoReplyAsync(text, query);
            await _botClient.EditMessageTextAsync(query.Message!, text, replyMarkup: null);
        }

        /// <summary>
        /// 显示命令回复
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("SAY", UserRights.NormalCmd)]
        public async Task ResponseSay(CallbackQuery query, string[] args)
        {
            string text;
            if (args.Length < 1)
            {
                text = "参数有误";
            }
            else
            {
                text = string.Join(' ', args[1..]);
            }
            await _botClient.AutoReplyAsync(text, query);
        }

        /// <summary>
        /// 获取随机稿件
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("RANDOMPOST", UserRights.NormalCmd, Description = "获取随机稿件")]
        public async Task GetRandomPost(Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.AdminCmd))
            {
                if (message.Chat.Type != ChatType.Private)
                {
                    await _botClient.SendCommandReply("该功能仅限私聊使用", message, autoDelete: false);
                    return;
                }
            }

            var keyboard = _markupHelperService.RandomPostMenuKeyboard(dbUser);
            await _botClient.SendCommandReply("手气不错, 获取随机稿件", message, autoDelete: false, replyMarkup: keyboard);
        }

        /// <summary>
        /// 获取随机稿件
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("RANDOMPOST", UserRights.NormalCmd)]
        public async Task QGetRandomPostp(Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            if (args.Length < 2)
            {
                await _botClient.EditMessageTextAsync(callbackQuery.Message!, "参数有误", replyMarkup: null);
                return;
            }

            BuildInTags tag = args[1] switch
            {
                "all" => BuildInTags.None,
                "nsfw" => BuildInTags.NSFW,
                "friend" => BuildInTags.Friend,
                "wanan" => BuildInTags.WanAn,
                "ai" => BuildInTags.AIGraph,
                _ => BuildInTags.None
            };

            string tagName = tag switch
            {
                BuildInTags.None => "",
                BuildInTags.NSFW => "NSFW",
                BuildInTags.Friend => "我有一个朋友",
                BuildInTags.WanAn => "晚安",
                BuildInTags.AIGraph => "AI怪图",
                _ => "",
            };

            var randomPost = await _postService.Queryable()
                        .WhereIF(tag == BuildInTags.None, x => x.Status == PostStatus.Accepted && x.PostType == MessageType.Photo)
                        .WhereIF(tag != BuildInTags.None, x => x.Status == PostStatus.Accepted && x.PostType == MessageType.Photo && ((byte)x.Tags & (byte)tag) > 0)
                        .OrderBy(x => SqlFunc.GetRandom()).Take(1).FirstAsync();

            if (randomPost != null)
            {
                var keyboard = _markupHelperService.RandomPostMenuKeyboard(dbUser, randomPost, tagName, args[1]);

                bool hasSpoiler = randomPost.Tags.HasFlag(BuildInTags.Spoiler);

                long chatId = callbackQuery.Message!.Chat.Id;

                if (randomPost.IsMediaGroup && false)
                {
                    var attachments = await _attachmentService.Queryable().Where(x => x.PostID == randomPost.Id).ToListAsync();
                    var group = new IAlbumInputMedia[attachments.Count];
                    for (int i = 0; i < attachments.Count; i++)
                    {
                        MessageType attachmentType = attachments[i].Type;
                        if (attachmentType == MessageType.Unknown)
                        {
                            attachmentType = randomPost.PostType;
                        }
                        group[i] = attachmentType switch
                        {
                            MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                            MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html },
                            MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                            MessageType.Voice => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html },
                            MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html },
                            _ => throw new Exception(),
                        };
                    }

                    var messages = await _botClient.SendMediaGroupAsync(chatId, group);
                }
                else
                {
                    Attachments attachment = await _attachmentService.Queryable().FirstAsync(x => x.PostID == randomPost.Id);
                    var handler = randomPost.PostType switch
                    {
                        MessageType.Photo => _botClient.SendPhotoAsync(chatId, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudioAsync(chatId, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideoAsync(chatId, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoiceAsync(chatId, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard),
                        MessageType.Document => _botClient.SendDocumentAsync(chatId, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard),
                        MessageType.Animation => _botClient.SendDocumentAsync(chatId, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard),
                        _ => null,
                    };

                    if (handler == null)
                    {
                        await _botClient.AutoReplyAsync($"不支持的稿件类型: {randomPost.PostType}", callbackQuery);
                        await _botClient.EditMessageTextAsync(callbackQuery.Message!, $"不支持的稿件类型: {randomPost.PostType}", null);
                        return;
                    }

                    var message = await handler;
                }

                //去除第一条消息的按钮
                var kbd = args.Length > 2 ? _markupHelperService.LinkToOriginPostKeyboard(args[2]) : null;
                await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, kbd);
            }
            else
            {
                await _botClient.EditMessageTextAsync(callbackQuery.Message!, "无可用稿件", replyMarkup: null);
            }
        }
    }
}
