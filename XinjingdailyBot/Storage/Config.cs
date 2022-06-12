namespace XinjingdailyBot.Storage
{
    public class Config
    {
        /// <summary>
        /// 调试模式
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// 日志等级
        /// </summary>
        public int LogLevel { get; set; } = 0;

        /// <summary>
        /// 机器人Token
        /// </summary>
        public string BotToken { get; set; } = "";

        /// <summary>
        /// 代理
        /// </summary>
        public string Proxy { get; set; } = "";

        /// <summary>
        /// 数据库设定
        /// </summary>
        public string DBHost { get; set; } = "127.0.0.1";
        public uint DBPort { get; set; } = 3306;
        public string DBName { get; set; } = "xjb_db";
        public string DBUser { get; set; } = "root";
        public string DBPassword { get; set; } = "123456";
        public bool DBGenerate { get; set; } = true;

        /// <summary>
        /// 超级管理员(覆盖数据库配置)
        /// </summary>
        public HashSet<int> SuperAdmins { get; set; } = new();

        /// <summary>
        /// 审核群组
        /// </summary>
        public string ReviewGroup { get; set; } = "";

        /// <summary>
        /// 通过频道
        /// </summary>
        public string AcceptChannel { get; set; } = "";

        /// <summary>
        /// 拒稿频道
        /// </summary>
        public string RejectChannel { get; set; } = "";
    }
}
