using XinjingDaily.Bot.Infrastructure.Configs;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.Infrastructure;

/// <summary>
/// 机器人配置
/// </summary>
public sealed record OptionSettings
{
    /// <inheritdoc cref="SystemConfig"/>
    public SystemConfig System { get; set; } = new();

    /// <inheritdoc cref="BotConfig"/>
    public BotConfig Bot { get; set; } = new();

    /// <inheritdoc cref="ChannelOption"/>
    public ChannelOption Channel { get; set; } = new();

    /// <inheritdoc cref="MessageOption"/>
    public MessageOption Message { get; set; } = new();

    /// <inheritdoc cref="RedisConfig"/>
    public RedisConfig Redis { get; set; } = new();

    /// <inheritdoc cref="DatabaseConfig"/>
    public DatabaseConfig Database { get; set; } = new();

    /// <inheritdoc cref="PostOption"/>
    public PostOption Post { get; set; } = new();

    /// <inheritdoc cref="GitHubOption"/>
    public GitHubOption GitHub { get; set; } = new();

    /// <inheritdoc cref="IpInfoOption"/>
    public IpInfoOption IpInfo { get; set; } = new();

    /// <inheritdoc cref="ScheduleOption"/>
    public ScheduleOption Schedule { get; set; } = new();

    /// <inheritdoc cref="LevelOption"/>
    public LevelOption Level { get; set; } = new();


    /// <summary>
    /// 频道选项
    /// </summary>
    public sealed record ChannelOption
    {
        /// <summary>
        /// 审核群组
        /// </summary>
        public string ReviewGroup { get; set; } = "";
        /// <summary>
        /// 日志频道
        /// </summary>
        public string LogChannel { get; set; } = "";
        /// <summary>
        /// 是否使用审核日志模式
        /// 启用: 审核后在审核群直接删除消息, 审核记录发送至审核日志频道
        /// 禁用: 审核后再审核群保留消息记录, 审核日志频道不使用
        /// </summary>
        public bool UseReviewLogMode { get; set; }
        /// <summary>
        /// 频道评论区群组
        /// </summary>
        public string CommentGroup { get; set; } = "";
        /// <summary>
        /// 闲聊区群组
        /// </summary>
        public string SubGroup { get; set; } = "";
        /// <summary>
        /// 通过频道
        /// </summary>
        public string AcceptChannel { get; set; } = "";
        /// <summary>
        /// 第二频道
        /// </summary>
        public string SecondChannel { get; set; } = "";
        /// <summary>
        /// 第二频道评论区
        /// </summary>
        public string SecondCommentGroup { get; set; } = "";
        /// <summary>
        /// 拒稿频道
        /// </summary>
        public string RejectChannel { get; set; } = "";
        /// <summary>
        /// 管理日志频道
        /// 用于存储封禁/解封日志
        /// </summary>
        public string AdminLogChannel { get; set; } = "";
    }

    /// <summary>
    /// 消息选项
    /// </summary>
    public sealed record MessageOption
    {
        /// <summary>
        /// /start 命令返回的消息
        /// </summary>
        public string? Start { get; set; }
        /// <summary>
        /// /help 命令返回的消息
        /// </summary>
        public string? Help { get; set; }
        /// <summary>
        /// /about 命令返回的消息
        /// </summary>
        public string? About { get; set; }
    }

    /// <summary>
    /// 稿件选项
    /// </summary>
    public sealed record PostOption
    {
        /// <summary>
        /// 启用每日投稿限制
        /// </summary>
        public bool EnablePostLimit { get; set; }
        /// <summary>
        /// 待定稿件上限, 不受 Ratio 倍率影响
        /// </summary>
        public int DailyPaddingLimit { get; set; } = 5;
        /// <summary>
        /// 审核队列上限
        /// </summary>
        public int DailyReviewLimit { get; set; } = 5;
        /// <summary>
        /// 每日投稿上限
        /// </summary>
        public int DailyPostLimit { get; set; } = 5;
        /// <summary>
        /// Ratio = 通过稿件数量 / RatioDivisor + 1
        /// 实际上限 = Ratio * 原始上限
        /// </summary>
        public int RatioDivisor { get; set; } = 100;
        /// <summary>
        /// 最高倍数
        /// </summary>
        public int MaxRatio { get; set; } = 10;
        /// <summary>
        /// 过滤连续空格
        /// </summary>
        public bool PureReturns { get; set; } = true;
        /// <summary>
        /// 过滤其他 #Tag
        /// </summary>
        public bool PureHashTag { get; set; } = true;
        /// <summary>
        /// 过滤字符串
        /// </summary>
        public string? PureWords { get; set; }
        /// <summary>
        /// 过滤字符串
        /// </summary>
        public List<string> PureWordsList { get; set; } = [];
        /// <summary>
        /// 稿件自动过期时间
        /// </summary>
        public uint PostExpiredTime { get; set; } = 3;
    }

    /// <summary>
    /// GitHub选项
    /// </summary>
    public sealed record GitHubOption
    {
        /// <summary>
        /// Github Api地址
        /// </summary>
        public string? BaseUrl { get; set; }
    }

    /// <summary>
    /// IpInfo选项
    /// </summary>
    public sealed record IpInfoOption
    {
        /// <summary>
        /// Token
        /// </summary>
        public string? Token { get; set; }
    }

    /// <summary>
    /// 任务计划
    /// </summary>
    public sealed record ScheduleOption
    {
        /// <summary>
        /// 任务计划
        /// </summary>
        public Dictionary<string, string?> Cron { get; set; } = [];
    }

    /// <summary>
    /// 等级经验值设定
    /// </summary>
    public sealed record LevelOption
    {
        /// <summary>
        /// 每个通过稿件获得的经验
        /// </summary>
        public float ExpPerAccept { get; set; }
        /// <summary>
        /// 每个拒绝稿件获得的经验
        /// </summary>
        public float ExpPerReject { get; set; }
        /// <summary>
        /// 每个审核稿件获得的经验
        /// </summary>
        public float ExpPerReview { get; set; }
        /// <summary>
        /// 每个过期稿件获得的经验
        /// </summary>
        public float ExpPerExpire { get; set; }
    }
}
