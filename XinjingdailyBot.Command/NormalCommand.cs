using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Net;
using System.Text;
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

namespace XinjingdailyBot.Command;

/// <summary>
/// 通用命令
/// </summary>
[AppService(LifeTime.Scoped)]
internal class NormalCommand
{
    private readonly ILogger<NormalCommand> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly GroupRepository _groupRepository;
    private readonly IMarkupHelperService _markupHelperService;
    private readonly IAttachmentService _attachmentService;
    private readonly IPostService _postService;
    private readonly TagRepository _tagRepository;
    private readonly IHttpHelperService _httpHelperService;
    private readonly IMediaGroupService _mediaGroupService;

    public NormalCommand(
        ILogger<NormalCommand> logger,
        ITelegramBotClient botClient,
        IUserService userService,
        GroupRepository groupRepository,
        IMarkupHelperService markupHelperService,
        IAttachmentService attachmentService,
        IPostService postService,
        TagRepository tagRepository,
        IHttpHelperService httpHelperService,
        IMediaGroupService mediaGroupService)
    {
        _logger = logger;
        _botClient = botClient;
        _userService = userService;
        _groupRepository = groupRepository;
        _markupHelperService = markupHelperService;
        _attachmentService = attachmentService;
        _postService = postService;
        _tagRepository = tagRepository;
        _httpHelperService = httpHelperService;
        _mediaGroupService = mediaGroupService;
    }

    /// <summary>
    /// 检测机器人是否存活
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("PING", EUserRights.NormalCmd, Description = "检测机器人是否存活")]
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
    [TextCmd("ANONYMOUS", EUserRights.NormalCmd, Alias = "ANYMOUSE", Description = "设置是否匿名")]
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
        await _userService.Updateable(dbUser).UpdateColumns(static x => new { x.PreferAnonymous, x.ModifyAt }).ExecuteCommandAsync();

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
    [TextCmd("NOTIFICATION", EUserRights.NormalCmd, Description = "设置稿件审核后是否通知")]
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
        await _userService.Updateable(dbUser).UpdateColumns(static x => new { x.Notification, x.ModifyAt }).ExecuteCommandAsync();

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
    [TextCmd("MYINFO", EUserRights.NormalCmd, Description = "获取自己的信息")]
    public async Task ResponseMyInfo(Users dbUser, Message message)
    {
        var sb = new StringBuilder();

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
    [TextCmd("MYRIGHT", EUserRights.NormalCmd, Description = "获取自己的权限")]
    public async Task ResponseMyRight(Users dbUser, Message message)
    {
        var right = dbUser.Right;
        bool superCmd = right.HasFlag(EUserRights.SuperCmd);
        bool adminCmd = right.HasFlag(EUserRights.AdminCmd);
        bool normalCmd = right.HasFlag(EUserRights.NormalCmd);
        bool sendPost = right.HasFlag(EUserRights.SendPost);
        bool reviewPost = right.HasFlag(EUserRights.ReviewPost);
        bool directPost = right.HasFlag(EUserRights.DirectPost);
        string userNick = message.From!.EscapedNickName();

        string group = _groupRepository.GetGroupName(dbUser.GroupID);

        var functions = new List<string>();
        if (sendPost) { functions.Add("投递稿件"); }
        if (reviewPost) { functions.Add("审核稿件"); }
        if (directPost) { functions.Add("直接发布稿件"); }
        if (functions.Count == 0) { functions.Add("无"); }

        var commands = new List<string>();
        if (superCmd) { commands.Add("所有命令"); }
        if (adminCmd) { commands.Add("管理员命令"); }
        if (normalCmd) { commands.Add("普通命令"); }
        if (functions.Count == 0) { commands.Add("无"); }

        var sb = new StringBuilder();
        sb.AppendLine($"用户名: <code>{userNick}</code>");
        sb.AppendLine($"用户组: <code>{group}</code>");
        sb.AppendLine($"功能: <code>{string.Join(", ", functions)}</code>");
        sb.AppendLine($"命令: <code>{string.Join(", ", commands)}</code>");

        await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
    }

    /// <summary>
    /// 艾特群管理
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("ADMIN", EUserRights.NormalCmd, Description = "艾特群管理")]
    public async Task ResponseCallAdmins(Users dbUser, Message message)
    {
        if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
        {
            await _botClient.SendCommandReply("该命令仅在群组内有效", message, false, ParseMode.Html);
        }
        else
        {
            var sb = new StringBuilder();
            var admins = await _botClient.GetChatAdministratorsAsync(message.Chat.Id);
            foreach (var menber in admins)
            {
                var admin = menber.User;
                if (!(admin.IsBot || string.IsNullOrEmpty(admin.Username)))
                {
                    sb.AppendLine($"@{admin.Username}");
                }
            }

            var msg = await _botClient.SendCommandReply(sb.ToString(), message, false, ParseMode.Html);
            await Task.Delay(2000);

            try
            {
                await _botClient.EditMessageTextAsync(msg, $"用户 {dbUser} 呼叫群管理", ParseMode.Html);
            }
            catch (Exception)
            {
                _logger.LogWarning("编辑消息失败");
            }
        }
    }

    /// <summary>
    /// 取消命令
    /// </summary>
    /// <param name="query"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    [QueryCmd("CANCEL", EUserRights.NormalCmd, Alias = "CANCELCLOSE CANCELANDCLOSE")]
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
    [QueryCmd("SAY", EUserRights.NormalCmd)]
    public async Task QResponseSay(CallbackQuery query, string[] args)
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
    [TextCmd("RANDOMPOST", EUserRights.NormalCmd, Description = "获取随机稿件")]
    public async Task ResponseRandomPost(Users dbUser, Message message)
    {
        if (!dbUser.Right.HasFlag(EUserRights.AdminCmd))
        {
            if (message.Chat.Type != ChatType.Private)
            {
                await _botClient.SendCommandReply("该功能仅限私聊使用", message, autoDelete: false);
                return;
            }
        }
        var keyboard = _markupHelperService.RandomPostMenuKeyboard(dbUser);
        await _botClient.SendCommandReply("请选择感兴趣的稿件标签", message, autoDelete: false, replyMarkup: keyboard);
    }

    /// <summary>
    /// 获取随机稿件, 设置标签
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="callbackQuery"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    [QueryCmd("SETRANDOMPOST", EUserRights.NormalCmd)]
    public async Task QResponseSetRandomPost(Users dbUser, CallbackQuery callbackQuery, string[] args)
    {
        if (args.Length < 1)
        {
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, "参数有误", replyMarkup: null);
            return;
        }

        int tagId = 0;
        string text = "从【所有】已通过的稿件中获取:";

        if (args.Length > 1)
        {
            var tag = _tagRepository.GetTagByPayload(args[1]);
            if (tag != null)
            {
                tagId = tag.Id;
                text = $"从带有【{tag.HashTag}】标签的稿件中获取:";
            }
        }

        var keyboard = _markupHelperService.RandomPostMenuKeyboard(dbUser, tagId);
        await _botClient.EditMessageTextAsync(callbackQuery.Message!, text, replyMarkup: keyboard);
    }

    /// <summary>
    /// 获取随机稿件, 返回上一级
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="callbackQuery"></param>
    /// <returns></returns>
    [QueryCmd("BACKRANDOMPOST", EUserRights.NormalCmd)]
    public async Task QResponseBackRandomPost(Users dbUser, CallbackQuery callbackQuery)
    {
        var keyboard = _markupHelperService.RandomPostMenuKeyboard(dbUser);
        await _botClient.EditMessageTextAsync(callbackQuery.Message!, "请选择感兴趣的稿件标签", replyMarkup: keyboard);
    }

    /// <summary>
    /// 获取随机稿件
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="callbackQuery"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    [QueryCmd("RANDOMPOST", EUserRights.NormalCmd)]
    public async Task QGetRandomPost(Users dbUser, CallbackQuery callbackQuery, string[] args)
    {
        if (args.Length < 3 || !int.TryParse(args[1], out int tagId))
        {
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, "参数有误", replyMarkup: null);
            return;
        }

        MessageType? postType = args[2] switch {
            "photo" => MessageType.Photo,
            "video" => MessageType.Video,
            "audio" => MessageType.Audio,
            "animation" => MessageType.Animation,
            "document" => MessageType.Document,
            "all" => null,
            _ => null,
        };

        var tag = tagId != 0 ? _tagRepository.GetTagById(tagId) : null;

        var randomPost = await _postService.Queryable()
                    .Where(static x => x.Status == EPostStatus.Accepted)
                    .WhereIF(postType == null, static x => x.PostType != MessageType.Text)
                    .WhereIF(postType != null, x => x.PostType == postType)
                    .WhereIF(tag != null, x => (x.Tags & tag!.Seg) > 0)
                    .OrderBy(static x => SqlFunc.GetRandom()).FirstAsync();

        if (randomPost != null)
        {
            var keyboard = _markupHelperService.RandomPostMenuKeyboard(dbUser, randomPost, tagId, args[2]);
            bool hasSpoiler = randomPost.HasSpoiler;
            var chat = callbackQuery.Message!.Chat;

            if (randomPost.IsMediaGroup)
            {
                var attachments = await _attachmentService.FetchAttachmentsByPostId(randomPost.Id);
                var group = new IAlbumInputMedia[attachments.Count];
                for (int i = 0; i < attachments.Count; i++)
                {
                    var attachmentType = attachments[i].Type;
                    if (attachmentType == MessageType.Unknown)
                    {
                        attachmentType = randomPost.PostType;
                    }
                    group[i] = attachmentType switch {
                        MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Voice => new InputMediaVideo(new InputFileId(attachments[i].FileID)) { Caption = i == 0 ? randomPost.Text : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)) { Caption = i == attachments.Count - 1 ? randomPost.Text : null, ParseMode = ParseMode.Html },
                        _ => throw new Exception(),
                    };
                }

                var messages = await _botClient.SendMediaGroupAsync(chat, group);
                await _botClient.SendTextMessageAsync(chat, "随机稿件操作", replyMarkup: keyboard);

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(messages);
            }
            else
            {
                var attachment = await _attachmentService.FetchAttachmentByPostId(randomPost.Id);
                var handler = randomPost.PostType switch {
                    MessageType.Text => _botClient.SendTextMessageAsync(chat, randomPost.Text),
                    MessageType.Photo => _botClient.SendPhotoAsync(chat, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard, hasSpoiler: hasSpoiler),
                    MessageType.Audio => _botClient.SendAudioAsync(chat, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard, title: attachment.FileName),
                    MessageType.Video => _botClient.SendVideoAsync(chat, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard, hasSpoiler: hasSpoiler),
                    MessageType.Voice => _botClient.SendVoiceAsync(chat, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard),
                    MessageType.Document => _botClient.SendDocumentAsync(chat, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard),
                    MessageType.Animation => _botClient.SendAnimationAsync(chat, new InputFileId(attachment.FileID), caption: randomPost.Text, parseMode: ParseMode.Html, replyMarkup: keyboard),
                    _ => null,
                };

                if (handler == null)
                {
                    await _botClient.AutoReplyAsync($"不支持的稿件类型: {randomPost.PostType}", callbackQuery, true);
                    await _botClient.EditMessageTextAsync(callbackQuery.Message!, $"不支持的稿件类型: {randomPost.PostType}", null);
                    return;
                }

                var message = await handler;
            }

            //去除第一条消息的按钮
            if (args.Length > 3 && long.TryParse(args[3], out long msgId))
            {
                var kbd = _markupHelperService.LinkToOriginPostKeyboard(msgId);
                await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, kbd);
            }
            else
            {
                await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, null);
            }
        }
        else
        {
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, "无可用稿件", replyMarkup: null);
        }
    }

    [TextCmd("IP", EUserRights.NormalCmd, Alias = "IPINFO", Description = "查询IP信息")]
    public async Task GetIpInfo(Message message, string[] args)
    {
        var sb = new StringBuilder();
        if (args.Length < 1)
        {
            sb.AppendLine("参数错误, 请指定要查询的IP");
        }
        else
        {
            if (IPAddress.TryParse(args[0], out var ip))
            {
                var response = await _httpHelperService.GetIpInformation(ip);
                if (response == null)
                {
                    sb.AppendLine("读取出错");
                }
                else
                {
                    sb.AppendLine($"IP: <code>{response.Ip}</code>");
                    sb.AppendLine($"主机: <code>{response.Ip}</code>");
                    sb.AppendLine($"城市: <code>{response.City}</code>");
                    sb.AppendLine($"地区: <code>{response.Region}</code>");
                    sb.AppendLine($"国家: <code>{response.Country}</code>");
                    sb.AppendLine($"坐标: <code>{response.Loc}</code>");
                    sb.AppendLine($"ASN: <code>{response.Org}</code>");
                    sb.AppendLine($"邮编: <code>{response.Postal}</code>");
                    sb.AppendLine($"时区: <code>{response.Timezone}</code>");
                }
            }
            else
            {
                sb.AppendLine("参数错误, 输入的IP无效");
            }
        }
        await _botClient.AutoReplyAsync(sb.ToString(), message, ParseMode.Html);
    }
}
