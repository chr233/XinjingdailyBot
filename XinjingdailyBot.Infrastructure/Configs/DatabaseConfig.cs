using DsNext.Infrastructure.Options;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record DatabaseConfig : IXjbConfig
{
    static string? IXjbConfig.SectionName => "Database";

    /// <summary>
    /// 是否生成数据库字段(数据库结构变动时需要打开), 默认 false
    /// </summary>
    public bool Generate { get; set; }
    /// <summary>
    /// 打印SQL日志
    /// </summary>
    public bool LogSQL { get; set; }
    /// <summary>
    /// 是否使用MySQL数据库, true:MySQL, false:SQLite
    /// </summary>
    [Obsolete("使用DbType代替")]
    public bool UseMySQL { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string? DbType { get; set; }
    /// <summary>
    /// 数据库连接字符串(DbType选Custom时生效)
    /// </summary>
    public string? DbConnectionString { get; set; }
    /// <summary>
    /// MySQL主机IP
    /// </summary>
    public string? DbHost { get; set; }
    /// <summary>
    /// MySQL主机端口
    /// </summary>
    public uint DbPort { get; set; }
    /// <summary>
    /// MySQL数据库名称
    /// </summary>
    public string? DbName { get; set; }
    /// <summary>
    /// MySQL用户名
    /// </summary>
    public string? DbUser { get; set; }
    /// <summary>
    /// MySQL密码
    /// </summary>
    public string? DbPassword { get; set; }
}
