using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Localization;

namespace XinjingdailyBot.Helpers
{
    internal sealed class MarkupHelper
    {
        private static string AnymouseOn = Emoji.Ghost + "匿名投稿";
        private static string AnymouseOff = Emoji.Thinking + "署名投稿";
        private static string PostCancel = Emoji.No + "取消";
        private static string PostConfirm = Emoji.Yes + "投稿";

        private static string TagNSFW = Emoji.Orange + "NSFW";
        private static string TagFriend = Emoji.Orange + "我有一个朋友";
        private static string TagWanAn = Emoji.Orange + "晚安";

        private static string ReviewReject = Emoji.No + "拒绝";
        private static string ReviewAccept = Emoji.Yes + "采用";

        private static string RejectFuzzy = Emoji.Blue + "图片模糊";
        private static string RejectDuplicate = Emoji.Blue + "重复稿件";
        private static string RejectBoring = Emoji.Blue + "无趣";
        private static string RejectDeny = Emoji.Blue + "内容不合适";
        private static string RejectOther = Emoji.Blue + "其他原因";

        private static string RejectCancel = Emoji.Blue + "返回";

        internal static InlineKeyboardMarkup PostKeyboard(bool anymouse)
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData( anymouse ? AnymouseOn : AnymouseOff, "post anymouse"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( PostCancel,  "post cancel"),
                    InlineKeyboardButton.WithCallbackData( PostConfirm,  "post confirm"),
                },
            });
            return keyboard;
        }

        internal static InlineKeyboardMarkup ReviewKeyboardA()
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(TagNSFW, "review tag nsfw"),
                    InlineKeyboardButton.WithCallbackData(TagFriend, "review tag friend"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( ReviewReject,  "review reject"),
                    InlineKeyboardButton.WithCallbackData( ReviewAccept,  "review accept"),
                },
            });
            return keyboard;
        }

        internal static InlineKeyboardMarkup ReviewKeyboardB()
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(RejectFuzzy, "reject fuzzy"),
                    InlineKeyboardButton.WithCallbackData(RejectDuplicate, "reject duplicate"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(RejectBoring, "reject boring"),
                    InlineKeyboardButton.WithCallbackData(RejectDeny, "reject deny"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(RejectOther, "reject other"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( RejectCancel,  "reject back"),
                },
            });
            return keyboard;
        }
    }
}
