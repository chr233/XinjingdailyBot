using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    [AppService(LifeTime.Singleton)]
    public class RejectReasonRepository : BaseRepository<RejectReason>
    {
        private readonly ILogger<RejectReasonRepository> _logger;

        public RejectReasonRepository(ILogger<RejectReasonRepository> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 拒绝理由缓存, Key=Id
        /// </summary>
        private Dictionary<int, RejectReason> RejectReasonCache { get; } = new();
        /// <summary>
        /// 标签缓存, Key=Payload
        /// </summary>
        private Dictionary<string, RejectReason> RejectReasonPayloadCache { get; } = new();

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
            if (reasons?.Count > 0)
            {
                RejectReasonCache.Clear();
                RejectReasonPayloadCache.Clear();

                foreach (var reason in reasons)
                {
                    if (string.IsNullOrEmpty(reason.Name) || reason.Id <= 0)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(reason.Payload))
                    {
                        reason.Payload = reason.Name;
                    }
                    reason.Payload = reason.Payload.ToLowerInvariant();

                    RejectReasonCache.Add(reason.Id, reason);
                    RejectReasonPayloadCache.Add(reason.Payload, reason);
                }

                await Storageable(reasons).ExecuteCommandAsync();

                _logger.LogInformation("已加载 {Count} 个拒绝理由", reasons.Count);
            }
            else
            {
                _logger.LogError("意外错误, 拒绝理由数据为空");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 创建内置群组
        /// </summary>
        /// <returns></returns>
        private async Task InsertBuildInRejectReasons()
        {
            //请不要修改ID为0和1的字段
            var levels = new List<RejectReason>()
            {
                new() { Id = 1, Name = "模糊", Payload = "fuzzy", FullText = "图片模糊/看不清", IsCount = false },
                new() { Id = 2, Name = "重复", Payload = "duplicate", FullText = "重复的稿件", IsCount = false },
                new() { Id = 3, Name = "无趣", Payload = "boring", FullText = "内容不够有趣" },
                new() { Id = 4, Name = "没懂", Payload = "confusing", FullText = "审核没看懂,建议配文说明" },
                new() { Id = 5, Name = "内容不合适", Payload = "deny", FullText = "不合适发布的内容" },
                new() { Id = 6, Name = "广告水印", Payload = "qrcode", FullText = "稿件包含二维码水印" },
                new() { Id = 7, Name = "其他原因", Payload = "other", FullText = "其他原因" },
                new() { Id = 8, Name = "太多了", Payload = "toomuch", FullText = "太多了" },
            };

            await Storageable(levels).ExecuteCommandAsync();
        }

        public RejectReason? GetReasonById(int reasonId)
        {
            if (RejectReasonCache.TryGetValue(reasonId, out var level))
            {
                return level;
            }
            return null;
        }

        /// <summary>
        /// 根据Payload获取标签
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public RejectReason? GetTagByPayload(string payload)
        {
            if (RejectReasonPayloadCache.TryGetValue(payload, out var reason))
            {
                return reason;
            }
            return null;
        }

        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RejectReason> GetAllTags()
        {
            return RejectReasonCache.Values;
        }
    }
}
