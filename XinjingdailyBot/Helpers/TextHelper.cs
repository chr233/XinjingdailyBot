using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Helpers
{
    internal sealed class TextHelper
    {
        private static readonly Regex MatchTag = new(@"(^#\S+)|(\s#\S+)");
        private static readonly Regex MatchSpace = new(@"^\s*$");

        /// <summary>
        /// 去除无用文本内容
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PureText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            text = MatchTag.Replace(text, "");

            var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Where(x => !MatchSpace.IsMatch(x)).Select(x => x.Trim());

            return string.Join('\n', parts);
        }

        /// <summary>
        /// 提取Tag
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BuildInTags FetchTags(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BuildInTags.None;
            }

            BuildInTags tags = BuildInTags.None;

            if (text.Contains("NSFW", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.NSFW;
            }
            if (text.Contains("朋友", StringComparison.InvariantCultureIgnoreCase) || text.Contains("英雄", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.Friend;
            }
            if (text.Contains("晚安", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.WanAn | BuildInTags.NSFW;
            }
            return tags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static string HtmlUserLink(Users user)
        {
            string userNick;
            if (string.IsNullOrEmpty(user.LastName))
            {
                userNick = user.FirstName;
            }
            else
            {
                userNick = $"{user.FirstName} {user.LastName}";
            }
            return HtmlUserLink(user.UserID, user.UserName, userNick);
        }

        internal static string HtmlUserLink(long userId, string userName, string userNick)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return $"<a href=\"tg://user?id={userId}\">{userNick}</a>";
            }
            else
            {
                return $"<a href=\"https://t.me/{userName}\">{userNick}</a>";
            }
        }
    }
}
