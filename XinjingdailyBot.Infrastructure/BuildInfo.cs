#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

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
}
