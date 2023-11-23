using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IUserService"/>
[AppService(typeof(IUserService), LifeTime.Singleton)]
internal sealed class UserService : BaseService<Users>, IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly OptionsSetting _optionsSetting;
    private readonly GroupRepository _groupRepository;
    private readonly IMarkupHelperService _markupHelperService;
    private readonly IChannelService _channelService;
    private readonly ICmdRecordService _cmdRecordService;
    private readonly PostRepository _postRepository;
    private readonly ITelegramBotClient _botClient;
    private readonly LevelRepository _levelRepository;
    private readonly INameHistoryService _nameHistoryService;
    private readonly IMediaGroupService _mediaGroupService;

    public UserService(
        ILogger<UserService> logger,
        IOptions<OptionsSetting> configuration,
        GroupRepository groupRepository,
        IMarkupHelperService markupHelperService,
        IChannelService channelService,
        ICmdRecordService cmdRecordService,
        PostRepository postRepository,
        ITelegramBotClient botClient,
        LevelRepository levelRepository,
        INameHistoryService nameHistoryService,
        IMediaGroupService mediaGroupService,
        ISqlSugarClient context) : base(context)
    {
        _logger = logger;
        _optionsSetting = configuration.Value;
        _groupRepository = groupRepository;
        _markupHelperService = markupHelperService;
        _channelService = channelService;
        _cmdRecordService = cmdRecordService;
        _postRepository = postRepository;
        _botClient = botClient;
        _levelRepository = levelRepository;
        _nameHistoryService = nameHistoryService;
        _mediaGroupService = mediaGroupService;
    }

    /// <summary>
    /// 更新周期
    /// </summary>
    private static readonly TimeSpan UpdatePeriod = TimeSpan.FromDays(14);

    public async Task<Users?> FetchUserFromUpdate(Update update)
    {
        var msgChat = update.Type switch {
            UpdateType.ChannelPost => update.ChannelPost!.Chat,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat,
            UpdateType.Message => update.Message!.Chat,
            UpdateType.EditedMessage => update.EditedMessage!.Chat,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat,
            _ => null
        };

        await AutoLeaveChat(msgChat);

        var message = update.Type switch {
            UpdateType.Message => update.Message!,
            UpdateType.ChannelPost => update.ChannelPost!,
            _ => null,
        };

        // 自动删除置顶通知 和 群名修改通知
        if (message != null && (message.Type == MessageType.MessagePinned || message.Type == MessageType.ChatTitleChanged))
        {
            await AutoDeleteNotification(message);
            return null;
        }

        if (update.Type == UpdateType.ChannelPost)
        {
            return await QueryUserFromChannelPost(update.ChannelPost!);
        }
        else
        {
            var msgUser = update.Type switch {
                UpdateType.ChannelPost => update.ChannelPost!.From,
                UpdateType.EditedChannelPost => update.EditedChannelPost!.From,
                UpdateType.Message => update.Message!.From,
                UpdateType.EditedMessage => update.EditedMessage!.From,
                UpdateType.CallbackQuery => update.CallbackQuery!.From,
                UpdateType.InlineQuery => update.InlineQuery!.From,
                UpdateType.ChosenInlineResult => update.ChosenInlineResult!.From,
                UpdateType.ChatJoinRequest => update.ChatJoinRequest!.From,
                _ => null
            };

            return await QueryUserFromChat(msgUser, msgChat);
        }
    }

    /// <summary>
    /// 自动退出无关群组
    /// </summary>
    /// <param name="msgChat"></param>
    /// <returns></returns>
    private async Task AutoLeaveChat(Chat? msgChat)
    {
        if (msgChat == null || !_optionsSetting.Bot.AutoLeaveOtherGroup)
        {
            return;
        }

        bool autoLeave = false;
        switch (msgChat.Type)
        {
            case ChatType.Group:
            case ChatType.Supergroup:
                if (!_channelService.IsGroupMessage(msgChat) && !_channelService.IsReviewMessage(msgChat))
                {
                    autoLeave = true;
                }
                break;
            case ChatType.Channel:
                if (!_channelService.IsChannelMessage(msgChat))
                {
                    autoLeave = true;
                }
                break;
            default:
                return;
        }

        if (autoLeave)
        {
            try
            {
                await _botClient.LeaveChatAsync(msgChat.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "退出群组失败");
            }
        }
    }

    /// <summary>
    /// 根据MessageUser获取用户
    /// </summary>
    /// <param name="msgUser"></param>
    /// <param name="msgChat"></param>
    /// <returns></returns>
    private async Task<Users?> QueryUserFromChat(User? msgUser, Chat? msgChat)
    {
        if (msgUser == null)
        {
            return null;
        }

        bool isDebug = _optionsSetting.Debug;

        if (msgUser.Username == "GroupAnonymousBot")
        {
            if (isDebug)
            {
                if (msgChat != null)
                {
                    _logger.LogDebug("忽略群匿名用户 {chatProfile}", msgChat.ChatProfile());
                }
            }
            return null;
        }

        var dbUser = await Queryable().FirstAsync(x => x.UserID == msgUser.Id);

        var chatID = msgChat?.Type == ChatType.Private ? msgChat.Id : -1;

        if (dbUser == null)
        {
            var defaultGroup = _groupRepository.GetDefaultGroup();

            if (defaultGroup == null)
            {
                _logger.LogError("未设置默认群组");
                return null;
            }

            dbUser = new Users {
                UserID = msgUser.Id,
                UserName = msgUser.Username ?? "",
                FirstName = msgUser.FirstName,
                LastName = msgUser.LastName ?? "",
                IsBot = msgUser.IsBot,
                IsBan = false,
                IsVip = false,
                GroupID = defaultGroup.Id,
                PrivateChatID = chatID,
                Right = defaultGroup.DefaultRight,
                Level = 1,
            };

            try
            {
                await Insertable(dbUser).ExecuteCommandAsync();
                if (isDebug)
                {
                    _logger.LogDebug("创建用户 {dbUser} 成功", dbUser);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户 {dbUser} 失败", dbUser);
                return null;
            }
        }
        else
        {
            var needUpdate = false;

            //用户名不一致时更新
            if (dbUser.UserName != (msgUser.Username ?? "")
                || dbUser.FirstName != msgUser.FirstName || dbUser.LastName != (msgUser.LastName ?? ""))
            {
                await _nameHistoryService.CreateNameHistory(dbUser);

                dbUser.UserName = msgUser.Username ?? "";
                dbUser.FirstName = msgUser.FirstName;
                dbUser.LastName = msgUser.LastName ?? "";
                needUpdate = true;
            }

            if (dbUser.IsBot != msgUser.IsBot)
            {
                dbUser.IsBot = msgUser.IsBot;
                needUpdate = true;
            }

            if (dbUser.PrivateChatID != chatID)
            {
                if (chatID != -1)
                {
                    dbUser.PrivateChatID = chatID;
                    needUpdate = true;
                }
            }

            //超过设定时间也触发更新
            if (DateTime.Now > dbUser.ModifyAt + UpdatePeriod)
            {
                needUpdate = true;
            }

            if (!_groupRepository.HasGroupId(dbUser.GroupID))
            {
                var defaultGroup = _groupRepository.GetDefaultGroup();
                if (defaultGroup == null)
                {
                    _logger.LogError("未设置默认群组");
                    return null;
                }
                dbUser.GroupID = defaultGroup.Id;
                needUpdate = true;
            }

            //需要更新用户数据
            if (needUpdate)
            {
                try
                {
                    dbUser.ModifyAt = DateTime.Now;
                    await Updateable(dbUser).UpdateColumns(static x => new {
                        x.UserName,
                        x.FirstName,
                        x.LastName,
                        x.IsBot,
                        x.GroupID,
                        x.PrivateChatID,
                        x.ModifyAt
                    }).ExecuteCommandAsync();
                    if (isDebug)
                    {
                        _logger.LogDebug("更新用户 {dbUser} 成功", dbUser);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "更新用户 {dbUser} 失败", dbUser);
                    return null;
                }
            }
        }

        //如果是配置文件中指定的管理员就覆盖用户组权限
        if (_optionsSetting.Bot.SuperAdmins?.Contains(dbUser.UserID) ?? false)
        {
            dbUser.GroupID = _groupRepository.GetMaxGroupId();
        }

        //根据GroupID设置用户权限信息 (封禁用户区别对待)
        var group = _groupRepository.GetGroupById(!dbUser.IsBan ? dbUser.GroupID : 0);

        if (group != null)
        {
            dbUser.Right = group.DefaultRight;
        }
        else
        {
            _logger.LogError("读取用户 {dbUser} 权限组 {GroupID} 失败", dbUser, dbUser.GroupID);
            return null;
        }

        return dbUser;
    }

    public async Task<Users?> QueryUserByUserId(long UserId)
    {
        var user = await Queryable().FirstAsync(x => x.UserID == UserId);
        return user;
    }

    /// <summary>
    /// 频道管理员缓存
    /// </summary>
    private readonly Dictionary<string, long> _channelUserIdCache = new();

    /// <summary>
    /// 根据ChannelPost Author获取用户
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<Users?> QueryUserFromChannelPost(Message message)
    {
        if (message.Chat.Id != _channelService.AcceptChannel.Id)
        {
            return null;
        }

        string? author = message.AuthorSignature;
        if (string.IsNullOrEmpty(author))
        {
            return null;
        }

        if (_channelUserIdCache.TryGetValue(author, out long userId))
        {
            return await QueryUserByUserId(userId);
        }
        else //缓存中没有该用户, 更新缓存
        {
            var admins = await _botClient.GetChatAdministratorsAsync(message.Chat);
            if (admins == null)
            {
                return null;
            }
            foreach (var admis in admins)
            {
                string name = admis.User.FullName();
                _channelUserIdCache[name] = admis.User.Id;
            }

            if (_channelUserIdCache.TryGetValue(author, out userId))
            {
                return await QueryUserByUserId(userId);
            }
        }
        return null;
    }

    /// <summary>
    /// 自动删除置顶消息通知
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task AutoDeleteNotification(Message message)
    {
        if (_channelService.IsChannelMessage(message.Chat) || _channelService.IsGroupMessage(message.Chat) || _channelService.IsReviewMessage(message.Chat))
        {
            try
            {
                await _botClient.DeleteMessageAsync(message.Chat, message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除置顶通知失败");
            }
        }
    }

    public async Task<Users?> FetchUserByUserID(long userID)
    {
        var dbUser = await Queryable().FirstAsync(x => x.UserID == userID);
        return dbUser;
    }

    public async Task<Users?> FetchUserByUserName(string? userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return null;
        }
        else
        {
            var dbUser = await Queryable().FirstAsync(x => x.UserName == userName);
            return dbUser;
        }
    }

    public async Task<Users?> FetchTargetUser(Message message)
    {
        if (message.ReplyToMessage == null)
        {
            return null;
        }

        var replyToMsg = message.ReplyToMessage;

        if (replyToMsg.From == null)
        {
            return null;
        }

        //被回复的消息是Bot发的消息
        if (replyToMsg.From.Id == _channelService.BotUser.Id)
        {
            var chatId = message.Chat.Id;
            var msgId = replyToMsg.MessageId;

            //在审核群内
            if (chatId == _channelService.ReviewGroup.Id)
            {
                NewPosts? post;

                var mediaGroup = await _mediaGroupService.QueryMediaGroup(chatId, msgId);
                if (mediaGroup == null)//单条稿件
                {
                    post = await _postRepository.Queryable().FirstAsync(x =>
                        (x.ReviewChatID == chatId && x.ReviewMsgID == msgId) || (x.ReviewActionChatID == chatId && x.ReviewActionMsgID == msgId)
                    );
                }
                else //媒体组稿件
                {
                    post = await _postRepository.Queryable().FirstAsync(x => x.ReviewChatID == chatId && x.ReviewMediaGroupID == mediaGroup.MediaGroupID);
                }

                //判断是不是审核相关消息
                if (post != null)
                {
                    //通过稿件读取用户信息
                    return await FetchUserByUserID(post.PosterUID);
                }
            }

            //在CMD回调表里查看
            var cmdAction = await _cmdRecordService.FetchCmdRecordByMessageId(replyToMsg.MessageId);
            if (cmdAction != null)
            {
                return await FetchUserByUserID(cmdAction.UserID);
            }

            return null;
        }

        //获取消息发送人
        return await FetchUserByUserID(replyToMsg.From.Id);
    }

    public async Task<Users?> FetchTargetUser(long chatId, int msgId)
    {
        //在审核群内
        if (chatId == _channelService.ReviewGroup.Id)
        {
            NewPosts? post;

            var mediaGroup = await _mediaGroupService.QueryMediaGroup(chatId, msgId);
            if (mediaGroup == null)//单条稿件
            {
                post = await _postRepository.Queryable().FirstAsync(x =>
                    (x.ReviewChatID == chatId && x.ReviewMsgID == msgId) || (x.ReviewActionChatID == chatId && x.ReviewActionMsgID == msgId)
                );
            }
            else //媒体组稿件
            {
                post = await _postRepository.Queryable().FirstAsync(x => x.ReviewChatID == chatId && x.ReviewMediaGroupID == mediaGroup.MediaGroupID);
            }

            //判断是不是审核相关消息
            if (post != null)
            {
                //通过稿件读取用户信息
                return await FetchUserByUserID(post.PosterUID);
            }

            return null;
        }

        //在CMD回调表里查看
        var cmdAction = await _cmdRecordService.FetchCmdRecordByMessageId(msgId);
        if (cmdAction != null)
        {
            return await FetchUserByUserID(cmdAction.UserID);
        }

        return null;
    }

    public async Task<Users?> FetchUserByUserNameOrUserID(string? target)
    {
        if (string.IsNullOrEmpty(target))
        {
            return null;
        }

        if (target.StartsWith('@'))
        {
            return await FetchUserByUserName(target[1..]);
        }


        if (long.TryParse(target, out var userID))
        {
            return await FetchUserByUserID(userID) ?? await FetchUserByUserName(target);
        }
        else
        {
            return await FetchUserByUserName(target);
        }
    }

    public async Task<(string, InlineKeyboardMarkup?)> QueryUserList(Users dbUser, string query, int page)
    {
        //每页数量
        const int pageSize = 30;

        var date = DateTime.Now.AddDays(-90);

        //SQL表达式
        var exp = Expressionable.Create<Users>();

        //查找全部
        if (query == "*")
        {
            exp.And(x => x.ModifyAt > date);
        }
        else
        {
            //根据userID查找用户
            if (long.TryParse(query, out var userID))
            {
                exp.Or(x => x.UserID == userID && x.ModifyAt > date);
            }

            //根据用户名查找用户
            exp.Or(x => (x.FirstName.Contains(query) || x.LastName.Contains(query)) && x.ModifyAt > date);

            //根据UserName查找用户
            if (query.StartsWith('@'))
            {
                query = query[1..];
            }
            exp.Or(x => x.UserName.Contains(query) && x.ModifyAt > date);
        }

        var userListCount = await Queryable().Where(exp.ToExpression()).CountAsync();

        if (userListCount == 0)
        {
            return ("找不到符合条件的用户, 如需查找全部用户, 请使用 /queryalluser", null);
        }

        var totalPages = userListCount / pageSize;
        if (userListCount % pageSize > 0)
        {
            totalPages++;
        }

        page = Math.Max(1, Math.Min(page, totalPages));

        var userList = await Queryable().Where(exp.ToExpression()).ToPageListAsync(page, pageSize);

        var sb = new StringBuilder();

        var start = 1 + (page - 1) * pageSize;
        var index = 0;
        foreach (var user in userList)
        {
            var url = user.HtmlUserLink();

            sb.Append($"{start + index++}. <code>{user.UserID}</code> {url}");

            if (!string.IsNullOrEmpty(user.UserName))
            {
                sb.Append($" <code>@{user.UserName}</code>");
            }
            if (user.IsBan)
            {
                sb.Append(" 已封禁");
            }
            if (user.IsBot)
            {
                sb.Append(" 机器人");
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine($"共 {userListCount} 条, 当前显示 {start}~{start + userList.Count - 1} 条, 仅查找90天内活跃的用户");

        var keyboard = _markupHelperService.UserListPageKeyboard(dbUser, query, page, totalPages);

        return (sb.ToString(), keyboard);
    }

    public async Task<(string, InlineKeyboardMarkup?)> QueryAllUserList(Users dbUser, string query, int page)
    {
        //每页数量
        const int pageSize = 30;

        //SQL表达式
        var exp = Expressionable.Create<Users>();

        //查找全部
        if (query == "*")
        {
            exp.And(x => true);
        }
        else
        {
            //根据userID查找用户
            if (long.TryParse(query, out var userID))
            {
                exp.Or(x => x.UserID == userID);
            }

            //根据用户名查找用户
            exp.Or(x => x.FirstName.Contains(query) || x.LastName.Contains(query));

            //根据UserName查找用户
            if (query.StartsWith('@'))
            {
                query = query[1..];
            }
            exp.Or(x => x.UserName.Contains(query));
        }

        var userListCount = await Queryable().Where(exp.ToExpression()).CountAsync();

        if (userListCount == 0)
        {
            return ("找不到符合条件的用户", null);
        }

        var totalPages = userListCount / pageSize;
        if (userListCount % pageSize > 0)
        {
            totalPages++;
        }

        page = Math.Max(1, Math.Min(page, totalPages));

        var userList = await Queryable().Where(exp.ToExpression()).ToPageListAsync(page, pageSize);

        var sb = new StringBuilder();

        var start = 1 + (page - 1) * pageSize;
        var index = 0;
        foreach (var user in userList)
        {
            var url = user.HtmlUserLink();

            sb.Append($"{start + index++}. <code>{user.UserID}</code> {url}");

            if (!string.IsNullOrEmpty(user.UserName))
            {
                sb.Append($" <code>@{user.UserName}</code>");
            }
            if (user.IsBan)
            {
                sb.Append(" 已封禁");
            }
            if (user.IsBot)
            {
                sb.Append(" 机器人");
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine($"共 {userListCount} 条, 当前显示 {start}~{start + userList.Count - 1} 条");

        var keyboard = _markupHelperService.UserListPageKeyboard(dbUser, query, page, totalPages);

        return (sb.ToString(), keyboard);
    }

    public string GetUserBasicInfo(Users dbUser)
    {
        var userNick = dbUser.FullName.EscapeHtml();
        var level = _levelRepository.GetLevelName(dbUser.Level);
        var group = _groupRepository.GetGroupName(dbUser.GroupID);
        var status = dbUser.IsBan ? "封禁中" : "正常";

        int totalPost = dbUser.PostCount - dbUser.ExpiredPostCount;
        double passPercent = 1.0 * dbUser.AcceptCount / totalPost;

        var sb = new StringBuilder();

        sb.AppendLine($"用户名: <code>{userNick}</code>");
        sb.AppendLine($"用户ID: <code>{dbUser.UserID}</code>");
        sb.AppendLine($"用户组: <code>{group}</code>");
        sb.AppendLine($"经验: <code>{dbUser.Experience}</code>");
        sb.AppendLine($"状态: <code>{status}</code>");
        sb.AppendLine($"等级:  <code>{level}</code>");
        sb.AppendLine($"投稿数量: <code>{totalPost}</code>");
        sb.AppendLine($"投稿通过率: <code>{passPercent:0.00%}</code>");
        sb.AppendLine($"通过数量: <code>{dbUser.AcceptCount}</code>");
        sb.AppendLine($"拒绝数量: <code>{dbUser.RejectCount}</code>");
        sb.AppendLine($"审核数量: <code>{dbUser.ReviewCount}</code>");

        return sb.ToString();
    }

    /// <summary>
    /// 进入排行榜所需的最低稿件数量
    /// </summary>
    private const int MiniumRankPost = 10;

    public async Task<string> GetUserRank(Users dbUser)
    {
        var now = DateTime.Now;
        var prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

        var sb = new StringBuilder();


        if (dbUser.GroupID == 1)
        {
            if (dbUser.AcceptCount >= MiniumRankPost)
            {
                int acceptCountRank = await Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > dbUser.AcceptCount && x.ModifyAt >= prev30Days).CountAsync() + 1;

                double ratio = 1.0 * dbUser.AcceptCount / dbUser.PostCount;
                int acceptRatioRank = await Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > MiniumRankPost && x.ModifyAt >= prev30Days)
                .Select(y => 100.0 * y.AcceptCount / y.PostCount).Where(x => x > ratio).CountAsync() + 1;

                sb.AppendLine($"通过数排名: <code>{acceptCountRank}</code>");
                sb.AppendLine($"通过率排名: <code>{acceptRatioRank}</code>");

                int activeUser = await Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.ModifyAt >= prev30Days).CountAsync();
                sb.AppendLine($"活跃用户总数: <code>{activeUser}</code>");
            }
            else
            {
                sb.AppendLine("稿件数量太少, 未进入排行榜");
            }
        }
        else
        {
            int acceptCountRank = await Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.AcceptCount > dbUser.AcceptCount && x.ModifyAt >= prev30Days).CountAsync() + 1;
            int reviewCountRank = await Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.ReviewCount > dbUser.ReviewCount && x.ModifyAt >= prev30Days).CountAsync() + 1;

            sb.AppendLine($"投稿数排名: <code>{acceptCountRank}</code>");
            sb.AppendLine($"审核数排名: <code>{reviewCountRank}</code>");

            int activeAdmin = await Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.ModifyAt >= prev30Days).CountAsync();
            sb.AppendLine($"活跃管理员总数: <code>{activeAdmin}</code>");
        }

        return sb.ToString();
    }

    public async Task BanUser(Users targetUser, bool isBan)
    {
        targetUser.IsBan = isBan;
        targetUser.ModifyAt = DateTime.Now;
        await Updateable(targetUser).UpdateColumns(static x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 计算用户经验
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private ulong CalcUserExp(Users user)
    {
        var level = _optionsSetting.Level;
        var exp =
            level.ExpPerAccept * user.AcceptCount +
            level.ExpPerReject * user.RejectCount +
            level.ExpPerReview * user.ReviewCount +
            level.ExpPerExpire * user.ExpiredPostCount;
        return Convert.ToUInt64(exp);
    }

    public async Task UpdateUserPostCount(Users targerUser)
    {
        targerUser.Experience = CalcUserExp(targerUser);
        var level = _levelRepository.GetLevelByExp(targerUser.Experience);
        targerUser.Level = level?.Id ?? 1;

        await Updateable(targerUser).UpdateColumns(static x => new {
            x.Experience,
            x.Level,
            x.PostCount,
            x.AcceptCount,
            x.RejectCount,
            x.ExpiredPostCount,
            x.ReviewCount,
            x.ModifyAt
        }).ExecuteCommandAsync();
    }

    public Task<int> CountUser()
    {
        return Queryable().CountAsync();
    }

    public Task<int> CountRecentlyUpdateUser(DateTime afterDate)
    {
        return Queryable().Where(x => x.ModifyAt >= afterDate).CountAsync();
    }

    public Task<int> CountUnBannedUser()
    {
        return Queryable().Where(static x => !x.IsBan).CountAsync();
    }

    public Task<int> CountPostedUser()
    {
        return Queryable().Where(static x => x.PostCount > 0).CountAsync();
    }

    public Task<List<Users>> GetUserList(IEnumerable<long> userIds)
    {
        return Queryable().Where(x => userIds.Contains(x.UserID)).Distinct().ToListAsync();
    }

    public Task<List<Users>> GetUserListAfterId(int startId, int count)
    {
        return Queryable().Where(x => x.Id >= startId).Take(count).ToListAsync();
    }

    public Task<List<Users>> GetUserAcceptCountRankList(int miniumPost, DateTime miniumPostTime, int takeCount)
    {
        return Queryable()
            .Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > miniumPost && x.ModifyAt >= miniumPostTime)
            .OrderByDescending(static x => x.AcceptCount).Take(takeCount).ToListAsync();
    }

    public Task<List<Users>> GetAdminUserAcceptCountRankList(int miniumPost, DateTime miniumPostTime, int takeCount)
    {
        return Queryable()
            .Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.AcceptCount > miniumPost && x.ModifyAt >= miniumPostTime)
            .OrderByDescending(static x => x.AcceptCount).Take(takeCount).ToListAsync();
    }

    public Task<List<Users>> GetAdminUserReviewCountRankList(int miniumPost, DateTime miniumPostTime, int takeCount)
    {
        return Queryable()
            .Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.ReviewCount > miniumPost && x.ModifyAt >= miniumPostTime)
            .OrderByDescending(static x => x.ReviewCount).Take(takeCount).ToListAsync();
    }

    public Task UpdateUserGroupId(Users user, int groupId)
    {
        user.GroupID = groupId;
        user.ModifyAt = DateTime.Now;
        return Updateable(user).UpdateColumns(static x => new { x.GroupID, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task SetUserNotification(Users user, bool notification)
    {
        user.Notification = notification;
        user.ModifyAt = DateTime.Now;
        return Updateable(user).UpdateColumns(static x => new { x.Notification, x.ModifyAt }).ExecuteCommandAsync();
    }

    public Task SetUserPreferAnonymous(Users user, bool preferAnonymous)
    {
        user.PreferAnonymous = preferAnonymous;
        user.ModifyAt = DateTime.Now;
        return Updateable(user).UpdateColumns(static x => new { x.PreferAnonymous, x.ModifyAt }).ExecuteCommandAsync();
    }
}
