using System.Reflection;
using System.Runtime.InteropServices;

namespace XinjingdailyBot.Infrastructure
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 是否为Windows平台
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        /// <summary>
        /// 可执行文件名称
        /// </summary>
        public static string ExeFileName => IsWindows ? "XinjingdailyBot.WebAPI.exe" : "XinjingdailyBot.WebAPI";
        /// <summary>
        /// 可执行文件目录
        /// </summary>
        public static string ExeDirname => AppContext.BaseDirectory;
        /// <summary>
        /// 可执行文件路径
        /// </summary>
        public static string ExeFullPath => Path.Combine(ExeDirname, ExeFileName);
        /// <summary>
        /// 备份文件路径
        /// </summary>
        public static string BackupFullPath => Path.Combine(ExeDirname, ExeFileName + ".bak");
        /// <summary>
        /// 版本
        /// </summary>
        public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }
}
