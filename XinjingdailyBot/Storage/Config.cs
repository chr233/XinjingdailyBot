namespace XinjingdailyBot.Storage
{
    public class Config
    {
        /// <summary>
        /// 调试模式, 默认 false
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// 机器人Token
        /// </summary>
        public string BotToken { get; set; } = "";

        /// <summary>
        /// Start命令显示的欢迎语
        /// </summary>
        public string Welecome { get; set; } = "欢迎使用 心惊报 @xinjingdaily 专用投稿机器人";

        /// <summary>
        /// 忽略机器人离线时的Update
        /// </summary>
        public bool ThrowPendingUpdates { get; set; } = false;

        /// <summary>
        /// 代理链接, 默认 null
        /// </summary>
        public string? Proxy { get; set; }

        /// <summary>
        /// 是否使用MySQL数据库, true:MySQL, false:SQLite
        /// </summary>
        public bool UseMysql { get; set; } = true;

        /// <summary>
        /// MySQL连接设定
        /// </summary>
        public string DBHost { get; set; } = "127.0.0.1";
        public uint DBPort { get; set; } = 3306;
        public string DBName { get; set; } = "xjb_db";
        public string DBUser { get; set; } = "root";
        public string DBPassword { get; set; } = "123456";
        /// <summary>
        /// 是否生成数据库字段(数据库结构变动时需要打开), 默认 true
        /// </summary>
        public bool DBGenerate { get; set; } = true;

        /// <summary>
        /// 超级管理员(覆盖数据库配置)
        /// </summary>
        public HashSet<long> SuperAdmins { get; set; } = new();

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
}
