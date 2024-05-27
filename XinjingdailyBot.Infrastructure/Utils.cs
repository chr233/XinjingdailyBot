using System.Reflection;
using System.Runtime.Versioning;

namespace XinjingdailyBot.Infrastructure;

/// <summary>
/// 工具类
/// </summary>
public static class Utils
{
    /// <summary>
    /// 可执行文件
    /// </summary>
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    public static string AppDir => Directory.GetParent(_assembly.Location).FullName;
    public static string AppPath => _assembly.Location;

    /// <summary>
    /// 版本
    /// </summary>
    public static string? Version => _assembly.GetName().Version?.ToString();
    /// <summary>
    /// 公司
    /// </summary>
    public static string? Company => _assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
    /// <summary>
    /// 版权
    /// </summary>
    public static string? Copyright => _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
    /// <summary>
    /// 配置
    /// </summary>
    public static string? Configuration => _assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
    /// <summary>
    /// 框架
    /// </summary>
    public static string? FrameworkName => _assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName;

    /// <summary>
    /// 是否为调试模式
    /// </summary>
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif
}
