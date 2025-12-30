using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot;
using XinjingdailyBot.Interface.Bot.Common;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 拒稿存档频道置顶
/// </summary>
[Job("0 0 0 * * ?")]
internal class RejectChannelTask : IJob
{
    private readonly ILogger<RejectChannelTask> _logger;
    private readonly IChannelService _channelService;
    private readonly ITelegramBotService _botClient;

    public RejectChannelTask(
        ILogger<RejectChannelTask> logger,
        IChannelService channelService,
        ITelegramBotService botClient)
    {
        _logger = logger;
        _channelService = channelService;
        _botClient = botClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 置顶拒稿频道通知");

        var acceptChannel = _channelService.AcceptChannel;

        string descText = string.Format("此频道为 {0}({1}) 的附属频道\r\n此频道仅用于存档未通过的投稿, 频道中的内容均来自用户投稿\r\n本频道中的一切内容不代表 {0} 的立场", acceptChannel.Title, acceptChannel.ChatID());

        var rejectChannel = _channelService.RejectChannel;
        var message = await _botClient.SendMessage(rejectChannel.Id, descText).ConfigureAwait(false);
        await _botClient.PinChatMessage(rejectChannel, message.MessageId, true).ConfigureAwait(false);
    }
}
