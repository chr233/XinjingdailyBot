using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Extensions;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.IRepository.History.User;
using XinjingDaily.Bot.IRepository.User;

namespace XinjingDaily.Bot.Service.Storage;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class UserService(
    ILogger<UserService> _logger,
    IOptions<AppSettings> _options,
    IUserInfoRepository _userInfoRepository,
    IUserInfoHistoryRepository _userInfoHistoryRepository) : IUserService
{
    public async Task<UserInfo?> QueryUserFromUpdate(Update update)
    {
        var msgChat = update.Type switch {
            UpdateType.ChannelPost => update.ChannelPost!.Chat,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat,
            UpdateType.Message => update.Message!.Chat,
            UpdateType.EditedMessage => update.EditedMessage!.Chat,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat,
            _ => null
        };

        var message = update.Type switch {
            UpdateType.Message => update.Message!,
            UpdateType.ChannelPost => update.ChannelPost!,
            _ => null,
        };

        if (update.Type == UpdateType.ChannelPost)
        {
            return await QueryUserFromChannelPost(update.ChannelPost!).ConfigureAwait(false);
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

            return await QueryUserFromChat(msgUser, msgChat).ConfigureAwait(false);
        }
    }



    /// <summary>
    /// 根据ChannelPost Author获取用户
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<UserInfo?> QueryUserFromChannelPost(Message message)
    {
        //if (message.Chat.Id != _channelService.AcceptChannel.Id)
        //{
        //    return null;
        //}

        //string? author = message.AuthorSignature;
        //if (string.IsNullOrEmpty(author))
        //{
        //    return null;
        //}

        //if (_channelUserIdCache.TryGetValue(author, out long userId))
        //{
        //    return await QueryUserByUserId(userId).ConfigureAwait(false);
        //}
        //else //缓存中没有该用户, 更新缓存
        //{
        //    var admins = await _botClient.GetChatAdministrators(message.Chat).ConfigureAwait(false);
        //    if (admins == null)
        //    {
        //        return null;
        //    }
        //    foreach (var admis in admins)
        //    {
        //        string name = admis.User.FullName();
        //        _channelUserIdCache[name] = admis.User.Id;
        //    }

        //    if (_channelUserIdCache.TryGetValue(author, out userId))
        //    {
        //        return await QueryUserByUserId(userId).ConfigureAwait(false);
        //    }
        //}
        return null;
    }

    /// <summary>
    /// 根据MessageUser获取用户
    /// </summary>
    /// <param name="user"></param>
    /// <param name="chat"></param>
    /// <returns></returns>
    private async Task<UserInfo?> QueryUserFromChat(User? user, Chat? chat)
    {
        if (user == null)
        {
            return null;
        }

        bool isDebug = _options.Value.System.Debug;

        if (user.Username == "GroupAnonymousBot")
        {
            if (isDebug)
            {
                if (chat != null)
                {
                    _logger.LogDebug("忽略群匿名用户 {chatProfile}", chat.ChatProfile());
                }
            }
            return null;
        }

        var userInfo = await _userInfoRepository.QueryByTelegramIdAsync(user.Id).ConfigureAwait(false);
        if (userInfo == null)
        {
            userInfo = new UserInfo {
                TelegramId = user.Id,
                TelegramName = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsBot = user.IsBot,
                IsBan = false,
                CreateAt = DateTime.Now,
                ModifyAt = DateTime.MinValue,
            };

            try
            {
                await _userInfoRepository.InsertAsync(userInfo).ConfigureAwait(false);
                if (isDebug)
                {
                    _logger.LogDebug("创建用户 {user} 成功", userInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户 {user} 失败", userInfo);
                return null;
            }
        }
        else
        {
            var needUpdate = false;

            var nickChanged = userInfo.FirstName != user.FirstName || userInfo.LastName != user.LastName;
            var userNameChanged = userInfo.TelegramName != user.Username;

            //用户名不一致时更新
            if (nickChanged || userNameChanged)
            {
                userInfo.TelegramName = user.Username;
                userInfo.FirstName = user.FirstName;
                userInfo.LastName = user.LastName;

                await _userInfoHistoryRepository.CreateHistoryAsync(userInfo, nickChanged, userNameChanged).ConfigureAwait(false);

                needUpdate = true;
            }

            if (userInfo.IsBot != user.IsBot)
            {
                userInfo.IsBot = user.IsBot;
                needUpdate = true;
            }

            //    //超过设定时间也触发更新
            //    if (DateTime.Now > userInfo.ModifyAt + UpdatePeriod)
            //    {
            //        needUpdate = true;
            //    }

            //    if (!_groupRepository.HasGroupId(userInfo.GroupID))
            //    {
            //        var defaultGroup = _groupRepository.GetDefaultGroup();
            //        if (defaultGroup == null)
            //        {
            //            _logger.LogError("未设置默认群组");
            //            return null;
            //        }
            //        userInfo.GroupID = defaultGroup.Id;
            //        needUpdate = true;
            //    }

            //    //需要更新用户数据
            //    if (needUpdate)
            //    {
            //        try
            //        {
            //            userInfo.ModifyAt = DateTime.Now;
            //            await Updateable(userInfo).UpdateColumns(static x => new {
            //                x.UserName,
            //                x.FirstName,
            //                x.LastName,
            //                x.IsBot,
            //                x.GroupID,
            //                x.PrivateChatID,
            //                x.ModifyAt
            //            }).ExecuteCommandAsync().ConfigureAwait(false);
            //            if (isDebug)
            //            {
            //                _logger.LogDebug("更新用户 {dbUser} 成功", userInfo);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            _logger.LogError(ex, "更新用户 {dbUser} 失败", userInfo);
            //            return null;
            //        }
            //    }
            //}

            ////如果是配置文件中指定的管理员就覆盖用户组权限
            //if (_optionsSetting.Bot.SuperAdmins?.Contains(userInfo.UserID) ?? false)
            //{
            //    userInfo.GroupID = _groupRepository.GetMaxGroupId();
            //}

            ////根据GroupID设置用户权限信息 (封禁用户区别对待)
            //var group = _groupRepository.GetGroupById(!userInfo.IsBan ? userInfo.GroupID : 0);

            //if (group != null)
            //{
            //    userInfo.Right = group.DefaultRight;
            //}
            //else
            //{
            //    _logger.LogError("读取用户 {dbUser} 权限组 {GroupID} 失败", userInfo, userInfo.GroupID);
            //    return null;
            //}
        }

        return userInfo;
    }
}
