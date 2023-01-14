using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
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

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IUserService), LifeTime.Singleton)]
    public sealed class UserService : BaseService<Users>, IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly OptionsSetting _optionsSetting;
        private readonly GroupRepository _groupRepository;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IChannelService _channelService;
        private readonly ICmdRecordService _cmdRecordService;
        private readonly PostRepository _postRepository;
        private readonly ITelegramBotClient _botClient;

        public UserService(
            ILogger<UserService> logger,
            IOptions<OptionsSetting> configuration,
            GroupRepository groupRepository,
            IMarkupHelperService markupHelperService,
            IChannelService channelService,
            ICmdRecordService cmdRecordService,
           PostRepository postRepository,
           ITelegramBotClient botClient)
        {
            _logger = logger;
            _optionsSetting = configuration.Value;
            _groupRepository = groupRepository;
            _markupHelperService = markupHelperService;
            _channelService = channelService;
            _cmdRecordService = cmdRecordService;
            _postRepository = postRepository;
            _botClient = botClient;
        }

        /// <summary>
        /// 更新周期
        /// </summary>
        private static readonly TimeSpan UpdatePeriod = TimeSpan.FromDays(14);

        /// <summary>
        /// 根据Update获取发送消息的用户
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public async Task<Users?> FetchUserFromUpdate(Update update)
        {
            var msgChat = update.Type switch
            {
                UpdateType.ChannelPost => update.ChannelPost!.Chat,
                UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat,
                UpdateType.Message => update.Message!.Chat,
                UpdateType.EditedMessage => update.EditedMessage!.Chat,
                UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat,
                _ => null
            };

            await AutoLeaveChat(msgChat);

            if (update.Type == UpdateType.ChannelPost)
            {
                return await QueryUserFromChannelPost(update.ChannelPost!);
            }
            else
            {
                var msgUser = update.Type switch
                {
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
                    if (msgChat.Id != _channelService.SubGroup.Id &&
                        msgChat.Id != _channelService.CommentGroup.Id &&
                        msgChat.Id != _channelService.ReviewGroup.Id)
                    {
                        autoLeave = true;
                    }
                    break;
                case ChatType.Channel:
                    if (msgChat.Id != _channelService.AcceptChannel.Id &&
                        msgChat.Id != _channelService.RejectChannel.Id)
                    {
                        autoLeave = true;
                    }
                    break;
                default:
                    return;
            }

            if (autoLeave)
            {
                await _botClient.LeaveChatAsync(msgChat.Id);
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

                dbUser = new()
                {
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
                if (!(dbUser.UserName.Equals(msgUser.Username ?? "") && dbUser.FirstName.Equals(msgUser.FirstName) && dbUser.LastName.Equals(msgUser.LastName ?? "")))
                {
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
                        await Updateable(dbUser).UpdateColumns(x => new
                        {
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
                var maxGroupID = _groupRepository.GetMaxGroupId();
                dbUser.GroupID = maxGroupID;
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

        /// <summary>
        /// 查找用户
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
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
                var admins = await _botClient.GetChatAdministratorsAsync(message.Chat.Id);
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
        /// 根据UserID获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<Users?> FetchUserByUserID(long userID)
        {
            var dbUser = await Queryable().FirstAsync(x => x.UserID == userID);
            return dbUser;
        }

        /// <summary>
        /// 根据UserName获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 根据ReplyToMessage获取目标用户
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
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
                //在审核群内
                if (message.Chat.Id == _channelService.ReviewGroup.Id)
                {
                    var msgID = replyToMsg.MessageId;

                    var exp = Expressionable.Create<Posts>();
                    exp.Or(x => x.ManageMsgID == msgID);

                    if (string.IsNullOrEmpty(replyToMsg.MediaGroupId))
                    {
                        //普通消息
                        exp.Or(x => x.ReviewMsgID == msgID);
                    }
                    else
                    {
                        //媒体组消息
                        exp.Or(x => x.ReviewMsgID <= msgID && x.ManageMsgID > msgID);
                    }

                    var post = await _postRepository.Queryable().FirstAsync(exp.ToExpression());

                    //判断是不是审核相关消息
                    if (post != null)
                    {
                        //通过稿件读取用户信息
                        return await FetchUserByUserID(post.PosterUID);
                    }
                }

                //在CMD回调表里查看
                var cmdAction = await _cmdRecordService.Queryable().FirstAsync(x => x.MessageID == replyToMsg.MessageId);
                if (cmdAction != null)
                {
                    return await FetchUserByUserID(cmdAction.UserID);
                }

                return null;
            }

            //获取消息发送人
            return await FetchUserByUserID(replyToMsg.From.Id);
        }

        /// <summary>
        /// 根据用户输入查找指定用户
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 根据UserID查找指定用户
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<Users?> FetchDbUserByUserID(string? target)
        {
            if (long.TryParse(target, out var userID))
            {
                return await FetchUserByUserID(userID);
            }

            return null;
        }

        /// <summary>
        /// 查找用户
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<(string, InlineKeyboardMarkup?)> QueryUserList(Users dbUser, string query, int page)
        {
            //每页数量
            const int pageSize = 30;

            //SQL表达式
            var exp = Expressionable.Create<Users>();

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

            StringBuilder sb = new();

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
    }
}
