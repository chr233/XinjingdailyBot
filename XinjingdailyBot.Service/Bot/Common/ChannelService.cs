using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;

namespace XinjingdailyBot.Service.Bot.Common;

/// <inheritdoc cref="IChannelService"/>
[AppService(typeof(IChannelService), LifeTime.Singleton)]
public sealed class ChannelService(
        ILogger<ChannelService> _logger,
        ITelegramBotClient _botClient,
        IOptions<OptionsSetting> _options) : IChannelService
{
    private readonly OptionsSetting _optionsSetting = _options.Value;

    /// <inheritdoc/>
    public Chat ReviewGroup { get; private set; } = new();
    /// <inheritdoc/>
    public Chat? LogChannel { get; private set; }
    /// <inheritdoc/>
    public Chat CommentGroup { get; private set; } = new();
    /// <inheritdoc/>
    public Chat SubGroup { get; private set; } = new();
    /// <inheritdoc/>
    public Chat? SecondCommentGroup { get; private set; }
    /// <inheritdoc/>
    public Chat AcceptChannel { get; private set; } = new();
    /// <inheritdoc/>
    public Chat? SecondChannel { get; private set; }
    /// <inheritdoc/>
    public Chat RejectChannel { get; private set; } = new();
    /// <inheritdoc/>
    public Chat AdminLogChannel { get; private set; } = new();
    /// <inheritdoc/>
    public User BotUser { get; private set; } = new();

    /// <inheritdoc/>
    public async Task InitChannelInfo()
    {
        await _botClient.DeleteWebhookAsync(false).ConfigureAwait(false);

        BotUser = await _botClient.GetMeAsync().ConfigureAwait(false);

        _logger.LogInformation("机器人信息: {Id} {nickName} @{userName}", BotUser.Id, BotUser.FullName(), BotUser.Username);

        var channelOption = _optionsSetting.Channel;

        try
        {
            AcceptChannel = await _botClient.GetChatAsync(channelOption.AcceptChannel).ConfigureAwait(false);
            _logger.LogInformation("稿件发布频道: {chatProfile}", AcceptChannel.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的稿件发布频道, 请检查拼写是否正确");
            throw;
        }
        try
        {
            RejectChannel = await _botClient.GetChatAsync(channelOption.RejectChannel).ConfigureAwait(false);
            _logger.LogInformation("拒稿存档频道: {chatProfile}", RejectChannel.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的拒稿存档频道, 请检查拼写是否正确");
            throw;
        }
        if (!string.IsNullOrEmpty(channelOption.SecondChannel))
        {
            try
            {
                SecondChannel = await _botClient.GetChatAsync(channelOption.SecondChannel).ConfigureAwait(false);
                _logger.LogInformation("第二发布频道: {chatProfile}", SecondChannel.ChatProfile());
            }
            catch
            {
                _logger.LogError("未找到指定的稿件第二发布频道, 请检查拼写是否正确");
            }
        }

        try
        {
            if (long.TryParse(channelOption.ReviewGroup, out var groupId))
            {
                ReviewGroup = await _botClient.GetChatAsync(groupId).ConfigureAwait(false);
            }
            else
            {
                ReviewGroup = await _botClient.GetChatAsync(channelOption.ReviewGroup).ConfigureAwait(false);
            }
            _logger.LogInformation("审核群组: {chatProfile}", ReviewGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的审核群组, 可以使用 /groupinfo 命令获取群组信息");
            ReviewGroup = new Chat { Id = -1 };
        }

        if (channelOption.UseReviewLogMode)
        {
            //todo
        }

        try
        {
            if (long.TryParse(channelOption.CommentGroup, out var subGroupId))
            {
                CommentGroup = await _botClient.GetChatAsync(subGroupId).ConfigureAwait(false);
            }
            else
            {
                CommentGroup = await _botClient.GetChatAsync(channelOption.CommentGroup).ConfigureAwait(false);
            }
            _logger.LogInformation("评论区群组: {chatProfile}", CommentGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的评论区群组, 可以使用 /groupinfo 命令获取群组信息");
        }

        try
        {
            if (long.TryParse(channelOption.SubGroup, out var subGroupId))
            {
                SubGroup = await _botClient.GetChatAsync(subGroupId).ConfigureAwait(false);
            }
            else
            {
                SubGroup = await _botClient.GetChatAsync(channelOption.SubGroup).ConfigureAwait(false);
            }
            _logger.LogInformation("频道子群组: {chatProfile}", SubGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的闲聊群组, 可以使用 /groupinfo 命令获取群组信息");
            SubGroup = new Chat { Id = -1 };
        }

        try
        {
            if (long.TryParse(channelOption.SecondCommentGroup, out var subGroupId))
            {
                SecondCommentGroup = await _botClient.GetChatAsync(subGroupId).ConfigureAwait(false);
            }
            else
            {
                SecondCommentGroup = await _botClient.GetChatAsync(channelOption.SecondCommentGroup).ConfigureAwait(false);
            }
            _logger.LogInformation("第二频道评论区群组: {chatProfile}", SecondCommentGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的闲聊群组, 可以使用 /groupinfo 命令获取群组信息");
            SecondCommentGroup = new Chat { Id = -1 };
        }

        try
        {
            if (long.TryParse(channelOption.AdminLogChannel, out var adminLogChannelId))
            {
                AdminLogChannel = await _botClient.GetChatAsync(adminLogChannelId).ConfigureAwait(false);
            }
            else
            {
                AdminLogChannel = await _botClient.GetChatAsync(channelOption.AdminLogChannel).ConfigureAwait(false);
            }
            _logger.LogInformation("管理日志频道: {chatProfile}", AdminLogChannel.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的管理日志频道, 可以使用 /groupinfo 命令获取群组信息");
            SecondCommentGroup = new Chat { Id = -1 };
        }

        if (SubGroup.Id == -1 && CommentGroup.Id != -1)
        {
            SubGroup = CommentGroup;
        }
        else if (CommentGroup.Id == -1 && SubGroup.Id != -1)
        {
            CommentGroup = SubGroup;
        }
    }

    /// <inheritdoc/>
    public bool IsChannelMessage(long chatId)
    {
        return chatId == AcceptChannel.Id || chatId == SecondChannel?.Id || chatId == RejectChannel.Id;
    }
    /// <inheritdoc/>
    public bool IsChannelMessage(Chat chat)
    {
        return IsChannelMessage(chat.Id);
    }

    /// <inheritdoc/>
    public bool IsGroupMessage(long chatId)
    {
        return chatId == SubGroup.Id || chatId == CommentGroup.Id || chatId == SecondCommentGroup?.Id;
    }
    /// <inheritdoc/>
    public bool IsGroupMessage(Chat chat)
    {
        return IsGroupMessage(chat.Id);
    }

    /// <inheritdoc/>
    public bool IsReviewMessage(long chatId)
    {
        return chatId == ReviewGroup.Id;
    }
    /// <inheritdoc/>
    public bool IsReviewMessage(Chat chat)
    {
        return IsReviewMessage(chat.Id);
    }
    /// <inheritdoc/>
    public bool HasSecondChannel => SecondChannel != null;

    private static void UpdateChatTitle(Chat originChat, Chat targetChat)
    {
        targetChat.Title = originChat.Title;
        targetChat.Username = originChat.Username;
        targetChat.FirstName = originChat.FirstName;
        targetChat.LastName = originChat.LastName;
    }

    /// <inheritdoc/>
    public void OnChatTitleChanged(Chat chat, string? newChatTitle)
    {
        if (chat.Id == AcceptChannel.Id)
        {
            UpdateChatTitle(chat, AcceptChannel);
        }
        else if (chat.Id == RejectChannel.Id)
        {
            UpdateChatTitle(chat, RejectChannel);
        }
        else if (chat.Id == SecondChannel?.Id)
        {
            UpdateChatTitle(chat, SecondChannel);
        }
        else if (chat.Id == CommentGroup.Id)
        {
            UpdateChatTitle(chat, CommentGroup);
        }
        else if (chat.Id == SubGroup.Id)
        {
            UpdateChatTitle(chat, SubGroup);
        }
        else if (chat.Id == SecondCommentGroup?.Id)
        {
            UpdateChatTitle(chat, SecondCommentGroup);
        }
        else if (chat.Id == ReviewGroup.Id)
        {
            UpdateChatTitle(chat, ReviewGroup);
        }
        else if (chat.Id == LogChannel?.Id)
        {
            UpdateChatTitle(chat, LogChannel);
        }
    }
}
