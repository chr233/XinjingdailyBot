namespace XinjingdailyBot.Infrastructure
{
    public static class BuildInfo
    {
#if XJB_VARIANT_GENERIC
		    internal static bool CanUpdate => true;
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
        public static bool CanUpdate => true;
        public static string Variant => "win-x64";
#endif
    }
}
