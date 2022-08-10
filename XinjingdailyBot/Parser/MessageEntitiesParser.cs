using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Helpers;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Parser
{
    internal static class MessageEntitiesParser
    {
        /// <summary>
        /// 根据Message.Enetities的字段格式生成HTML文本, 自动过滤无用HashTag
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string ParseMessage(Message message)
        {
            MessageEntity[]? entities;
            string? text;

            if (message.Type == MessageType.Text)
            {
                text = message.Text;
                entities = message.Entities;
            }
            else
            {
                text = message.Caption;
                entities = message.CaptionEntities;
            }

            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (entities == null)
            {
                return text;
            }
            else
            {
                return ParseMessage(entities, text);
            }
        }

        /// <summary>
        /// 根据Message.Enetities的字段格式生成HTML文本, 自动过滤无用HashTag
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string ParseMessage(MessageEntity[] entities, string text)
        {
            StringBuilder sb = new(text.Replace('<', '＜').Replace('>', '＞').Replace('&', '＆'));

            Dictionary<int, TagObjct> tagMap = new();

            int count = entities.Length;


            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
                string head;
                string tail;

                switch (entity.Type)
                {
                    case MessageEntityType.Bold:
                        head = "<b>";
                        tail = "</b>";
                        break;
                    case MessageEntityType.Italic:
                        head = "<i>";
                        tail = "</i>";
                        break;
                    case MessageEntityType.Underline:
                        head = "<u>";
                        tail = "</u>";
                        break;
                    case MessageEntityType.Strikethrough:
                        head = "<s>";
                        tail = "</s>";
                        break;
                    case MessageEntityType.Spoiler:
                        head = "<tg-spoiler>";
                        tail = "</tg-spoiler>";
                        break;
                    case MessageEntityType.TextLink:
                        head = $"<a href=\"{TextHelper.EscapeHtml(entity.Url)}\">";
                        tail = "</a>";
                        break;
                    case MessageEntityType.Code:
                        head = "<code>";
                        tail = "</code>";
                        break;
                    case MessageEntityType.Pre:
                        head = "<pre>";
                        tail = "</pre>";
                        break;

                    default:
                        continue;
                }

                int start = entity.Offset;
                int end = entity.Offset + entity.Length;

                if (!tagMap.ContainsKey(start))
                {
                    tagMap.Add(start, new(head));
                }
                else
                {
                    tagMap[start].AddLast(head);
                }

                if (!tagMap.ContainsKey(end))
                {
                    tagMap.Add(end, new(tail));
                }
                else
                {
                    tagMap[end].AddFirst(tail);
                }
            }

            var indexList = tagMap.Keys.ToArray().OrderByDescending(x => x);

            foreach (var index in indexList)
            {
                sb.Insert(index, tagMap[index]);
            }

            Logger.Debug(sb.ToString());

            return TextHelper.PureText(sb.ToString());
        }
    }
}
