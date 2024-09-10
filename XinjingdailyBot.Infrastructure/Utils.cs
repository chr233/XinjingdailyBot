using NLog;

namespace XinjingdailyBot.Infrastructure;

/// <summary>
/// 工具类
/// </summary>
public static class Utils
{
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
