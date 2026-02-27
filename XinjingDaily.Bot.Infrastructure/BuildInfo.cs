#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

using System.Reflection;
using System.Runtime.Versioning;

namespace XinjingDaily.Bot.Infrastructure;

/// <summary>
/// 编译信息
/// </summary>
public static class BuildInfo
{
    static BuildInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Banner = @"
__  _ _             _  _            ___       _  _      
\ \/ <_>._ _  ___  <_><_>._ _  ___ | . \ ___ <_>| | _ _ 
 \ \ | || ' |/ . | | || || ' |/ . || | |<_> || || || | |
_/\_\|_||_|_|\_. | | ||_||_|_|\_. ||___/<___||_||_|`_. |
             <___'<__'        <___'                <___'
Dev";

        AppPath = assembly.Location;
        if (!string.IsNullOrEmpty(AppPath))
        {
            AppDir = Directory.GetParent(AppPath)?.FullName ?? ".";
        }
        else
        {
            AppDir = AppContext.BaseDirectory;
        }

        AppName = "XinjingDaily.Bot";

        Version = assembly.GetName().Version?.ToString()!;
        Company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company!;
        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright!;
        Configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration!;
        FrameworkName = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName!;
        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description!;
    }

    /// <summary>
    /// 标题
    /// </summary>
    public static string Banner { get; }

    /// <summary>
    /// Exe目录
    /// </summary>
    public static string AppDir { get; }
    /// <summary>
    /// Exe路径
    /// </summary>
    public static string AppPath { get; }
    /// <summary>
    /// Exe名称
    /// </summary>
    public static string AppName { get; }
    /// <summary>
    /// 版本
    /// </summary>
    public static string Version { get; }
    /// <summary>
    /// 公司
    /// </summary>
    public static string Company { get; }
    /// <summary>
    /// 版权
    /// </summary>
    public static string Copyright { get; }
    /// <summary>
    /// 编译配置
    /// </summary>
    public static string Configuration { get; }
    /// <summary>
    /// 框架版本
    /// </summary>
    public static string FrameworkName { get; }
    /// <summary>
    /// 描述
    /// </summary>
    public static string Description { get; }

    /// <summary>
    /// 是否为调试模式
    /// </summary>
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif

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
    public const string Author = "@chr233";
    public const string Repo = "https://github.com/chr233/XinjingDaily.Bot/";
}

