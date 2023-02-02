namespace XinjingdailyBot.Infrastructure
{
    public sealed record OptionsSetting
    {
        /// <summary>
        /// 调试模式
        /// </summary>
        public bool Debug { get; set; }
        /// <summary>
        /// 机器人配置
        /// </summary>
        public BotOption Bot { get; set; } = new();
        /// <summary>
        /// 频道配置
        /// </summary>
        public ChannelOption Channel { get; set; } = new();
        /// <summary>
        /// 消息配置
        /// </summary>
        public MessageOption Message { get; set; } = new();
        /// <summary>
        /// 数据库配置
        /// </summary>
        public DatabaseOption Database { get; set; } = new();
        /// <summary>
        /// 投稿配置
        /// </summary>
        public PostOption Post { get; set; } = new();
        
        public sealed record BotOption
        {
            /// <summary>
            /// 机器人Token
            /// </summary>
            public string? BotToken { get; set; }
            /// <summary>
            /// 代理链接, 默认 null
            /// </summary>
            public string? Proxy { get; set; }
            /// <summary>
            /// 忽略机器人离线时的Update
            /// </summary>
            public bool ThrowPendingUpdates { get; set; }
            /// <summary>
            /// 自动退出未在配置文件中定义的群组和频道, 默认 false
            /// </summary>
            public bool AutoLeaveOtherGroup { get; set; }
            /// <summary>
            /// 超级管理员(覆盖数据库配置)
            /// </summary>
            public HashSet<long>? SuperAdmins { get; set; }
        }

        public sealed record ChannelOption
        {
            /// <summary>
            /// 审核群组
            /// </summary>
            public string ReviewGroup { get; set; } = "";
            /// <summary>
            /// 审核日志频道
            /// </summary>
            public string ReviewLogChannel { get; set; } = "";
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
            /// 拒稿频道
            /// </summary>
            public string RejectChannel { get; set; } = "";
            /// <summary>
            /// 自动退出未在配置文件中定义的群组和频道, 默认 false
            /// </summary>
            public bool AutoLeaveOtherGroup { get; set; }
        }

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

        public sealed record DatabaseOption
        {
            /// <summary>
            /// 是否生成数据库字段(数据库结构变动时需要打开), 默认 false
            /// </summary>
            public bool Generate { get; set; }
            /// <summary>
            /// 是否使用MySQL数据库, true:MySQL, false:SQLite
            /// </summary>
            public bool UseMySQL { get; set; }
            /// <summary>
            /// 打印SQL日志
            /// </summary>
            public bool LogSQL { get; set; }
            /// <summary>
            /// MySQL连接设定
            /// </summary>
            public string? DbHost { get; set; }
            public int DbPort { get; set; }
            public string? DbName { get; set; }
            public string? DbUser { get; set; }
            public string? DbPassword { get; set; }
        }

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
        }
    }
}
