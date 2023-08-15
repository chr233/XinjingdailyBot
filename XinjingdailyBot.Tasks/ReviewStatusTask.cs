using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 定期发布稿件处理
/// </summary>
//[Job("0 0 0 * * ?")]
internal class ReviewStatusTask : IJob
{
    private readonly ILogger<ReviewStatusTask> _logger;
    private readonly IPostService _postService;
    private readonly IUserService _userService;
    private readonly ITelegramBotClient _botClient;
    private readonly TagRepository _tagRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IChannelService _channelService;
    private readonly IChannelOptionService _channelOptionService;
    private readonly ITextHelperService _textHelperService;
    private readonly IMediaGroupService _mediaGroupService;

    public ReviewStatusTask(
        ILogger<ReviewStatusTask> logger,
        IPostService postService,
        IUserService userService,
        ITelegramBotClient botClient,
        TagRepository tagRepository,
        IAttachmentService attachmentService,
        IChannelService channelService,
        IChannelOptionService channelOptionService,
        ITextHelperService textHelperService,
        IMediaGroupService mediaGroupService)
    {
        _logger = logger;
        _postService = postService;
        _userService = userService;
        _botClient = botClient;
        _tagRepository = tagRepository;
        _attachmentService = attachmentService;
        _channelService = channelService;
        _channelOptionService = channelOptionService;
        _textHelperService = textHelperService;
        _mediaGroupService = mediaGroupService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 更新投稿状态显示");

        var post = await _postService.Queryable()
            .Where(static x => x.Status == EPostStatus.InPlan).FirstAsync();

        try
        {

        }
        finally
        {
            post.Status = EPostStatus.Accepted;
            post.ModifyAt = DateTime.Now;

            await _postService.Updateable(post).UpdateColumns(static x => new {
                x.PublicMsgID,
                x.PublishMediaGroupID,
                x.Status,
                x.ModifyAt
            }).ExecuteCommandAsync();
        }
    }
}
