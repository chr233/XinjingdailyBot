namespace XinjingdailyBot.Infrastructure.Options;

/// <summary>
/// 数据库设置
/// </summary>
public sealed record DatabaseConfig
{
    /// <summary>
    /// 是否生成数据库字段(数据库结构变动时需要打开), 默认 false
    /// </summary>
    public bool Generate { get; init; }
    /// <summary>
    /// 打印SQL日志
    /// </summary>
    public bool LogSql { get; init; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string? Type { get; init; }
    /// <summary> 
    /// 自定义数据库连接字符串 (DbType 为 Custom 时生效)
    /// </summary>
    public string? CustomConnectionString { get; init; }
    /// <summary>
    /// 数据库主机IP
    /// </summary>
    public string? Host { get; init; }
    /// <summary>
    /// 数据库主机端口
    /// </summary>
    public uint Port { get; init; } = 3306;
    /// <summary>
    /// 数据库名称 (Sqlite 数据库名称)
    /// </summary>
    public string? Database { get; init; }
    /// <summary>
    /// 数据库用户名 (Sqlite忽略)
    /// </summary>
    public string? User { get; init; }
    /// <summary>
    /// 数据库密码 (Sqlite忽略)
    /// </summary>
    public string? Password { get; init; }

    public string? TablePrefix { get; set; }
}
