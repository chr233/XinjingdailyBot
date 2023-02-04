using System.Reflection;
using System.Runtime.InteropServices;

namespace XinjingdailyBot.Infrastructure
{
    /// <summary>
    /// 工具类
    /// </summary>
    public static class Utils
    {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static string ExeFileName => IsWindows ? "XinjingdailyBot.WebAPI.exe" : "XinjingdailyBot.WebAPI";
        public static string ExeDirname => AppContext.BaseDirectory;
        public static string ExeFullPath => Path.Combine(ExeDirname, ExeFileName);
        public static string BackupFullPath => Path.Combine(ExeDirname, ExeFileName + ".bak");
        public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }
}
