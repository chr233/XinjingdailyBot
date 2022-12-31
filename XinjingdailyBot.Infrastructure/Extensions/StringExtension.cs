namespace XinjingdailyBot.Infrastructure.Extensions
{
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
    }
}
