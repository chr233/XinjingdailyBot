using System.Text.RegularExpressions;

namespace XinjingdailyBot.Infrastructure;

/// <summary>
/// 正则工具类
/// </summary>
public static partial class RegexUtils
{
    /// <summary>
    /// 匹配HashTag
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("(^#\\S+)|(\\s#\\S+)", RegexOptions.Compiled)]
    public static partial Regex MatchHashTag();
    /// <summary>
    /// 匹配整个空行
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("^\\s*$", RegexOptions.Compiled)]
    public static partial Regex MatchBlankLine();

    /// <summary>
    /// 匹配链接Host
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"((:?https?:\/\/)?(:?[^/\s.#?]+(:?\.[^/\s.#?]+)+))", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    public static partial Regex MatchHttpLink();
}
