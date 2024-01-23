using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 定期发布稿件处理
/// </summary>
[Schedule("0 0 0 * * ?")]
public sealed class PlanedPostsTask(
    ILogger<PlanedPostsTask> _logger,
    IPostService _postService) : IJob
{
    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 发布定时稿件");

        var post = await _postService.GetInPlanPost();

        if (post == null)
        {
            _logger.LogInformation("无延时发布稿件");
            return;
        }

        var result = await _postService.PublicInPlanPost(post);
        _logger.LogInformation("发布定时稿件 {status}", result ? "成功" : "失败");
    }
}
