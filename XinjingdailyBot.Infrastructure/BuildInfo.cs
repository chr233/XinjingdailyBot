#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

using System.Reflection;
using System.Runtime.Versioning;

namespace XinjingdailyBot.Infrastructure;

/// <summary>
/// 编译信息
/// </summary>
public static class BuildInfo
{
#if XJB_GENERIC
	publish const bool CanUpdate = false;
	publish const string Variant = "generic";
#elif XJB_LINUX_ARM
	publish const bool CanUpdate = true;
	publish const string Variant = "linux-arm";
#elif XJB_LINUX_ARM64
	publish const bool CanUpdate = true;
	publish const string Variant = "linux-arm64";
#elif XJB_LINUX_X64
	publish const bool CanUpdate = true;
	publish const string Variant = "linux-x64";
#elif XJB_OSX_ARM64
	publish const bool CanUpdate = true;
	publish const string Variant = "osx-arm64";
#elif XJB_OSX_X64
	publish const bool CanUpdate = true;
	publish const string Variant = "osx-x64";
#elif XJB_WIN_ARM64
	publish const bool CanUpdate = true;
	public const string Variant = "win-arm64";
#elif XJB_WIN_X64
	public const bool CanUpdate = true;
	public const string Variant = "win-x64";
#else
    public const bool CanUpdate = false;
    public const string Variant = "source";
#endif
    public const string Author = "chr233";
    public const string Repo = "https://github.com/chr233/XinjingdailyBot/";

    /// <summary>
    /// 可执行文件
    /// </summary>
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    public static string AppDir => !string.IsNullOrEmpty(_assembly.Location) ? Directory.GetParent(_assembly.Location)?.FullName ?? "." : AppContext.BaseDirectory;
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
    /// 描述
    /// </summary>
    public static string? Description => _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

    /// <summary>
    /// 是否为调试模式
    /// </summary>
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif
}
