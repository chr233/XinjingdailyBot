using Microsoft.Extensions.Logging;
using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository;

/// <summary>
/// 拒绝理由仓储类
/// </summary>
[AppService(LifeTime.Singleton)]
public class RejectReasonRepository : BaseRepository<RejectReasons>
{
    private readonly ILogger<RejectReasonRepository> _logger;

    /// <summary>
    /// 拒绝理由仓储类
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="context"></param>
    public RejectReasonRepository(
        ILogger<RejectReasonRepository> logger,
        ISqlSugarClient context) : base(context)
    {
        _logger = logger;
    }

    /// <summary>
    /// 拒绝理由缓存, Key=Id
    /// </summary>
    private Dictionary<int, RejectReasons> RejectReasonCache { get; } = new();
    /// <summary>
    /// 标签缓存, Key=Payload
    /// </summary>
    private Dictionary<string, RejectReasons> RejectReasonPayloadCache { get; } = new();

    /// <summary>
    /// 初始化缓存
    /// </summary>
    /// <returns></returns>
    public async Task InitRejectReasonCache()
    {
        var reasonCount = await CountAsync(x => true);
        if (reasonCount == 0)
        {
            _logger.LogInformation("未设置拒绝理由，正在创建内置拒绝理由");
            await InsertBuildInRejectReasons();
        }

        var reasons = await GetListAsync();
        if (reasons.Any())
        {
            RejectReasonCache.Clear();
            RejectReasonPayloadCache.Clear();

            bool changed = false;

            foreach (var reason in reasons)
            {
                if (string.IsNullOrEmpty(reason.Name) || reason.Id <= 0)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(reason.Payload))
                {
                    reason.Payload = reason.Name;
                    changed = true;
                }
                reason.Payload = reason.Payload.ToLowerInvariant();

                RejectReasonCache.Add(reason.Id, reason);
                RejectReasonPayloadCache.Add(reason.Payload, reason);
            }

            if (changed)
            {
                await Storageable(reasons).ExecuteCommandAsync();
            }

            _logger.LogInformation("已加载 {Count} 个拒绝理由", reasons.Count);
        }
        else
        {
            _logger.LogError("意外错误, 拒绝理由数据为空");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// 创建内置拒绝理由
    /// </summary>
    /// <returns></returns>
    private async Task InsertBuildInRejectReasons()
    {
        var levels = new List<RejectReasons>
        {
            new RejectReasons { Id = 1, Name = "模糊", Payload = "fuzzy", FullText = "图片模糊/看不清", IsCount = false },
            new RejectReasons { Id = 2, Name = "重复", Payload = "duplicate", FullText = "重复的稿件", IsCount = false },
            new RejectReasons { Id = 3, Name = "无趣", Payload = "boring", FullText = "内容不够有趣" },
            new RejectReasons { Id = 4, Name = "没懂", Payload = "confusing", FullText = "审核没看懂,建议配文说明" },
            new RejectReasons { Id = 5, Name = "不合适", Payload = "deny", FullText = "不合适发布的内容" },
            new RejectReasons { Id = 6, Name = "广告水印", Payload = "qrcode", FullText = "稿件包含二维码水印" },
            new RejectReasons { Id = 7, Name = "其他原因", Payload = "other", FullText = "其他原因" },
            new RejectReasons { Id = 8, Name = "太多了", Payload = "toomuch", FullText = "今天此类型的稿件的数量太多了" },
        };

        await Storageable(levels).ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据Id获取拒绝理由
    /// </summary>
    /// <param name="reasonId"></param>
    /// <returns></returns>
    public RejectReasons? GetReasonById(int reasonId)
    {
        if (RejectReasonCache.TryGetValue(reasonId, out var level))
        {
            return level;
        }
        return null;
    }

    /// <summary>
    /// 根据Payload获取拒绝理由
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public RejectReasons? GetReasonByPayload(string payload)
    {
        if (RejectReasonPayloadCache.TryGetValue(payload, out var reason))
        {
            return reason;
        }
        return null;
    }

    /// <summary>
    /// 获取所有拒绝理由
    /// </summary>
    /// <returns></returns>
    public IEnumerable<RejectReasons> GetAllRejectReasons()
    {
        return RejectReasonCache.Values;
    }
}
