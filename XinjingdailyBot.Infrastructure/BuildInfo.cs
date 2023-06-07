#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace XinjingdailyBot.Infrastructure;

/// <summary>
/// 编译信息
/// </summary>
public static class BuildInfo
{
#if XJB_VARIANT_GENERIC
		    internal static bool CanUpdate => false;
		    internal static string Variant => "generic";
#elif XJB_VARIANT_LINUX_ARM
		    internal static bool CanUpdate => true;
		    internal static string Variant => "linux-arm";
#elif XJB_VARIANT_LINUX_ARM64
		    internal static bool CanUpdate => true;
		    internal static string Variant => "linux-arm64";
#elif XJB_VARIANT_LINUX_X64
		    internal static bool CanUpdate => true;
		    internal static string Variant => "linux-x64";
#elif XJB_VARIANT_OSX_ARM64
		    internal static bool CanUpdate => true;
		    internal static string Variant => "osx-arm64";
#elif XJB_VARIANT_OSX_X64
		    internal static bool CanUpdate => true;
		    internal static string Variant => "osx-x64";
#elif XJB_VARIANT_WIN_ARM64
		    internal static bool CanUpdate => true;
		    internal static string Variant => "win-arm64";
#elif XJB_VARIANT_WIN_X64
		    internal static bool CanUpdate => true;
		    internal static string Variant => "win-x64";
#else
    public static bool CanUpdate => false;
    public static string Variant => "source";
#endif
    public static string Author => "chr233";
    public static string Repo => "https://github.com/chr233/XinjingdailyBot/";
}
