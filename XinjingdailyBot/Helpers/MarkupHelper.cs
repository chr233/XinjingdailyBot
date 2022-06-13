using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Localization;

namespace XinjingdailyBot.Helpers
{
    internal sealed class MarkupHelper
    {
        private static readonly string AnymouseOn = Emoji.Ghost + "匿名投稿";
        private static readonly string AnymouseOff = Emoji.Thinking + "署名投稿";
        private static readonly string PostCancel = Emoji.No + "取消";
        private static readonly string PostConfirm = Emoji.Yes + "投稿";

        private static readonly string TagNSFW = "#NSFW";
        private static readonly string TagFriend =  "#我有一个朋友";
        private static readonly string TagWanAn =  "#晚安";

        private static readonly string ReviewReject = Emoji.No + "拒绝";
        private static readonly string ReviewAccept = Emoji.Yes + "采用";

        private static readonly string RejectFuzzy = "模糊";
        private static readonly string RejectDuplicate =  "重复";
        private static readonly string RejectBoring =  "无趣";
        private static readonly string RejectDeny =  "内容不合适";
        private static readonly string RejectOther =  "其他原因";

        private static readonly string RejectCancel = Emoji.Back + "返回";

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
                    InlineKeyboardButton.WithCallbackData(TagWanAn, "review tag wanan"),
                },
                new []
                {
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
                    InlineKeyboardButton.WithCallbackData(RejectBoring, "reject boring"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(RejectDeny, "reject deny"),
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
