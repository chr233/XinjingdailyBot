namespace DsNext.Infrastructure.Options;
public interface IXjbConfig
{
    /// <summary>
    /// 节名称, 留空使用根节点
    /// </summary>
    abstract static string? SectionName { get; }
}
