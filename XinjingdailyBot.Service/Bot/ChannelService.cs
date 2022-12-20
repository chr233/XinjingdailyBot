using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using SqlSugar.IOC;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot;
using XinjingdailyBot.Service.Data;

namespace XinjingdailyBot.Service.Bot;


[AppService(ServiceType = typeof(IChannelService), ServiceLifetime = LifeTime.Singleton)]
public class ChannelService : IChannelService
{
    private readonly ITelegramBotClient _botClient;
    private readonly OptionsSetting _optionsSetting;
    private readonly ILogger<ChannelService> _logger;

    private static Chat ReviewGroup = new();
    private static Chat CommentGroup = new();
    private static Chat SubGroup = new();
    private static Chat AcceptChannel = new();
    private static Chat RejectChannel = new();
    private static User BotUser = new();

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
        BotUser = await _botClient.GetMeAsync();

        _logger.LogInformation("机器人信息: {Id} {nickName} @{userName}", BotUser.Id, BotUser.NickName(), BotUser.Username);

        var channelOption = _optionsSetting.Channel;

        try
        {
            AcceptChannel = await _botClient.GetChatAsync(channelOption.AcceptChannel);
            _logger.LogInformation($"稿件发布频道: {AcceptChannel.ChatProfile()}");
        }
        catch
        {
            _logger.LogError("未找到指定的稿件发布频道, 请检查拼写是否正确");
            throw;
        }
        try
        {

            RejectChannel = await _botClient.GetChatAsync(channelOption.RejectChannel);
            _logger.LogInformation($"拒稿存档频道: {RejectChannel.ChatProfile()}");
        }
        catch
        {
            _logger.LogError("未找到指定的拒稿存档频道, 请检查拼写是否正确");
            throw;
        }

        try
        {
            if (long.TryParse(channelOption.ReviewGroup, out long groupId))
            {
                ReviewGroup = await _botClient.GetChatAsync(groupId);
            }
            else
            {
                ReviewGroup = await _botClient.GetChatAsync(channelOption.ReviewGroup);
            }
            _logger.LogInformation($"审核群组: {ReviewGroup.ChatProfile()}");
        }
        catch
        {
            _logger.LogError("未找到指定的审核群组, 可以使用 /groupinfo 命令获取群组信息");
            ReviewGroup = new() { Id = -1 };
        }

        try
        {
            if (long.TryParse(channelOption.CommentGroup, out long subGroupId))
            {
                CommentGroup = await _botClient.GetChatAsync(subGroupId);
            }
            else
            {
                CommentGroup = await _botClient.GetChatAsync(channelOption.CommentGroup);
            }
            _logger.LogInformation($"评论区群组: {CommentGroup.ChatProfile()}");
        }
        catch
        {
            _logger.LogError("未找到指定的评论区群组, 可以使用 /groupinfo 命令获取群组信息");
            CommentGroup = new() { Id = -1 };
        }

        try
        {
            if (long.TryParse(channelOption.SubGroup, out long subGroupId))
            {
                SubGroup = await _botClient.GetChatAsync(subGroupId);
            }
            else
            {
                SubGroup = await _botClient.GetChatAsync(channelOption.SubGroup);
            }
            _logger.LogInformation($"频道子群组: {SubGroup.ChatProfile()}");
        }
        catch
        {
            _logger.LogError("未找到指定的闲聊群组, 可以使用 /groupinfo 命令获取群组信息");
            SubGroup = new() { Id = -1 };
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

}
