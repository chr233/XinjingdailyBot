using NLog;
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

    /// <summary>
    /// 可执行文件所在目录
    /// </summary>
    public static string AppDir => Directory.GetParent(_assembly.Location)?.FullName ?? ".";
    /// <summary>
    /// 可执行文件路径
    /// </summary>
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

    /// <summary>
    /// 清除升级文件
    /// </summary>
    public static void CleanOldFiles()
    {
        var _logger = LogManager.GetCurrentClassLogger();

        var bakFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.bak");
        foreach (var bakPath in bakFiles)
        {
            try
            {
                File.Delete(bakPath);
                _logger.Warn("清理升级残留文件");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "清理升级残留文件失败");
            }
        }
    }
}
