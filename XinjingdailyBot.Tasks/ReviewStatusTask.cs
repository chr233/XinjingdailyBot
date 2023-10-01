using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 定期发布审核状态通知
/// </summary>
[Job("*/10 * * * ?")]
internal class ReviewStatusTask : IJob
{
    private readonly ILogger<ReviewStatusTask> _logger;
    private readonly IPostService _postService;
    private readonly ITelegramBotClient _botClient;
    private readonly IChannelService _channelService;
    private readonly IReviewStatusService _reviewStatusService;
    private readonly IMarkupHelperService _markupHelperService;

    public ReviewStatusTask(
        ILogger<ReviewStatusTask> logger,
        IPostService postService,
        ITelegramBotClient botClient,
        IChannelService channelService,
        IReviewStatusService reviewStatusService,
        IMarkupHelperService markupHelperService)
    {
        _logger = logger;
        _postService = postService;
        _botClient = botClient;
        _channelService = channelService;
        _reviewStatusService = reviewStatusService;
        _markupHelperService = markupHelperService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 更新投稿状态显示");

        var now = DateTime.Now;
        var prev1Day = now.AddDays(-1).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

        var todayPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status > EPostStatus.Cancel).CountAsync();
        var todayAcceptPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status == EPostStatus.Accepted).CountAsync();
        var todayRejectPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status == EPostStatus.Rejected).CountAsync();
        var todayPaddingPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status == EPostStatus.Reviewing).CountAsync();

        if (_channelService.HasSecondChannel)
        {
            var todayAcceptSecondPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status == EPostStatus.AcceptedSecond).CountAsync();
            todayAcceptPost += todayAcceptSecondPost;
        }

        var acceptRate = todayPost > 0 ? (100 * todayAcceptPost / todayPost).ToString("f2") : "--";
        var reviewRate = todayPost > 0 ? (100 * (todayPost - todayPaddingPost) / todayPost).ToString("f2") : "--";

        var sb = new StringBuilder();
        sb.AppendLine($"[审核统计 {now:HH:mm:ss}]");
        sb.AppendLine($"接受 <code>{todayAcceptPost}</code> 拒绝 <code>{todayRejectPost}</code> 待审核 <code>{todayPaddingPost}</code>");
        sb.AppendLine($"通过率: <code>{acceptRate}%</code> 审核率: <code>{reviewRate}%</code>");

        Message? statusMsg = null;

        var oldPost = await _reviewStatusService.GetOldReviewStatu();

        var reviewGroup = _channelService.ReviewGroup;
        var kbd = _markupHelperService.ReviewStatusButton();

        if (oldPost != null)
        {
            try
            {
                statusMsg = await _botClient.EditMessageTextAsync(reviewGroup, (int)oldPost.MessageID, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: kbd);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Bad Request: message is not modified"))
                {
                    return;
                }
                // 删除旧的消息
                await _reviewStatusService.DeleteOldReviewStatus();
                _logger.LogError(ex, "编辑消息失败");
            }
        }

        if (statusMsg == null)
        {
            statusMsg = await _botClient.SendTextMessageAsync(reviewGroup, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: kbd);
            await _botClient.PinChatMessageAsync(reviewGroup, statusMsg.MessageId);
            await _reviewStatusService.CreateNewReviewStatus(statusMsg);
        }
    }
}
