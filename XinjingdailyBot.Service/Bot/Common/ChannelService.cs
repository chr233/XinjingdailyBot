using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;

namespace XinjingdailyBot.Service.Bot.Common;

[AppService(typeof(IChannelService), LifeTime.Singleton)]

internal class ChannelService : IChannelService
{
    private readonly ITelegramBotClient _botClient;
    private readonly OptionsSetting _optionsSetting;
    private readonly ILogger<ChannelService> _logger;

    private Chat _reviewGroup = new();
    private Chat _reviewLogChannel = new();
    private Chat _commentGroup = new();
    private Chat _subGroup = new();
    private Chat _acceptChannel = new();
    private Chat _rejectChannel = new();
    private User _botUser = new();

    public Chat ReviewGroup { get => _reviewGroup; }
    public Chat ReviewLogChannel { get => _reviewLogChannel; }
    public Chat CommentGroup { get => _commentGroup; }
    public Chat SubGroup { get => _subGroup; }
    public Chat AcceptChannel { get => _acceptChannel; }
    public Chat RejectChannel { get => _rejectChannel; }
    public User BotUser { get => _botUser; }

    public ChannelService(
        ILogger<ChannelService> logger,
        ITelegramBotClient botClient,
        IOptions<OptionsSetting> optionsSetting)
    {
        _logger = logger;
        _botClient = botClient;
        _optionsSetting = optionsSetting.Value;
    }

    public async Task InitChannelInfo()
    {
        _botUser = await _botClient.GetMeAsync();

        _logger.LogInformation("机器人信息: {Id} {nickName} @{userName}", _botUser.Id, _botUser.FullName(), _botUser.Username);

        var channelOption = _optionsSetting.Channel;

        try
        {
            _acceptChannel = await _botClient.GetChatAsync(channelOption.AcceptChannel);
            _logger.LogInformation("稿件发布频道: {chatProfile}", _acceptChannel.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的稿件发布频道, 请检查拼写是否正确");
            throw;
        }
        try
        {

            _rejectChannel = await _botClient.GetChatAsync(channelOption.RejectChannel);
            _logger.LogInformation("拒稿存档频道: {chatProfile}", _rejectChannel.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的拒稿存档频道, 请检查拼写是否正确");
            throw;
        }

        try
        {
            if (long.TryParse(channelOption.ReviewGroup, out var groupId))
            {
                _reviewGroup = await _botClient.GetChatAsync(groupId);
            }
            else
            {
                _reviewGroup = await _botClient.GetChatAsync(channelOption.ReviewGroup);
            }
            _logger.LogInformation("审核群组: {chatProfile}", _reviewGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的审核群组, 可以使用 /groupinfo 命令获取群组信息");
        }

        if (channelOption.UseReviewLogMode)
        {
            //todo
        }

        try
        {
            if (long.TryParse(channelOption.CommentGroup, out var subGroupId))
            {
                _commentGroup = await _botClient.GetChatAsync(subGroupId);
            }
            else
            {
                _commentGroup = await _botClient.GetChatAsync(channelOption.CommentGroup);
            }
            _logger.LogInformation("评论区群组: {chatProfile}", _commentGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的评论区群组, 可以使用 /groupinfo 命令获取群组信息");
        }

        try
        {
            if (long.TryParse(channelOption.SubGroup, out var subGroupId))
            {
                _subGroup = await _botClient.GetChatAsync(subGroupId);
            }
            else
            {
                _subGroup = await _botClient.GetChatAsync(channelOption.SubGroup);
            }
            _logger.LogInformation("频道子群组: {chatProfile}", _subGroup.ChatProfile());
        }
        catch
        {
            _logger.LogError("未找到指定的闲聊群组, 可以使用 /groupinfo 命令获取群组信息");
            _subGroup = new Chat { Id = -1 };
        }

        if (_subGroup.Id == -1 && _commentGroup.Id != -1)
        {
            _subGroup = _commentGroup;
        }
        else if (_commentGroup.Id == -1 && _subGroup.Id != -1)
        {
            _commentGroup = _subGroup;
        }
    }

    public bool IsChannelMessage(long chatId)
    {
        return chatId == _acceptChannel.Id || chatId == _rejectChannel.Id;
    }
    public bool IsChannelMessage(Chat chat)
    {
        return chat.Id == _acceptChannel.Id || chat.Id == _rejectChannel.Id;
    }

    public bool IsGroupMessage(long chatId)
    {
        return chatId == _subGroup.Id || chatId == _commentGroup.Id;
    }
    public bool IsGroupMessage(Chat chat)
    {
        return chat.Id == _subGroup.Id || chat.Id == _commentGroup.Id;
    }

    public bool IsReviewMessage(long chatId)
    {
        return chatId == _reviewGroup.Id;
    }
    public bool IsReviewMessage(Chat chat)
    {
        return chat.Id == _reviewGroup.Id;
    }
}
