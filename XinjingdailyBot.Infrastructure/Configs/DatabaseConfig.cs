namespace XinjingdailyBot.Infrastructure.Configs;
public sealed record DatabaseConfig
{
    /// <summary>
    /// 是否生成数据库字段(数据库结构变动时需要打开), 默认 false
    /// </summary>
    public bool Generate { get; init; }
    /// <summary>
    /// 打印SQL日志
    /// </summary>
    public bool LogSQL { get; init; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string? Type { get; init; }
    /// <summary>
    /// 数据库连接字符串(DbType选Custom时生效)
    /// </summary>
    public string? ConnectionString { get; init; }
    /// <summary>
    /// MySQL主机IP
    /// </summary>
    public string? Host { get; init; }
    /// <summary>
    /// MySQL主机端口
    /// </summary>
    public uint Port { get; init; } = 3306;
    /// <summary>
    /// MySQL数据库名称
    /// </summary>
    public string? DbName { get; init; }
    /// <summary>
    /// MySQL用户名
    /// </summary>
    public string? User { get; init; }
    /// <summary>
    /// MySQL密码
    /// </summary>
    public string? Password { get; init; }
}
