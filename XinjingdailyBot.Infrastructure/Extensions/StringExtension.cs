namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// String扩展
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// HTML转义
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string EscapeHtml(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }
        else
        {
            var escapedText = text
                .Replace("<", "＜")
                .Replace(">", "＞")
                .Replace("&", "＆");
            return escapedText;
        }
    }

    /// <summary>
    /// HTML反转义
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ReEscapeHtml(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }
        else
        {
            var escapedText = text
                .Replace("＜", "&lt;")
                .Replace("＞", "&gt;")
                .Replace("＆", "&amp;");
            return escapedText;
        }
    }
}
