using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Helper
{
    [AppService(ServiceType = typeof(IMarkupHelperService), ServiceLifetime = LifeTime.Transient)]
    public sealed class MarkupHelperService : IMarkupHelperService
    {
        private readonly GroupRepository _groupRepository;


        private static readonly string AnymouseOn = Emojis.Ghost + "匿名投稿";
        private static readonly string AnymouseOff = Emojis.Thinking + "保留来源";
        private static readonly string PostCancel = Emojis.No + "取消";
        private static readonly string PostConfirm = Emojis.Yes + "投稿";

        private static readonly string TagNSFWOn = "#NSFW";
        private static readonly string TagFriendOn = "#我有一个朋友";
        private static readonly string TagWanAnOn = "#晚安";
        private static readonly string TagAIGraphOn = "#AI怪图";

        private static readonly string TagNSFWOff = "#N___";
        private static readonly string TagFriendOff = "#我_____";
        private static readonly string TagWanAnOff = "#晚_";
        private static readonly string TagAIGraphOff = "#A___";

        private static readonly string ReviewReject = Emojis.No + "拒绝";
        private static readonly string ReviewAccept = Emojis.Yes + "采用";

        private static readonly string RejectFuzzy = "模糊";
        private static readonly string RejectDuplicate = "重复";
        private static readonly string RejectBoring = "无趣";
        private static readonly string RejectConfusing = "没懂";
        private static readonly string RejectQRCode = "广告";
        private static readonly string RejectDeny = "内容不合适";
        private static readonly string RejectOther = "其他原因";

        private static readonly string RejectCancel = Emojis.Back + "返回";

        public MarkupHelperService(
            GroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        /// <summary>
        /// 投稿键盘
        /// </summary>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        public InlineKeyboardMarkup PostKeyboard(bool anymouse)
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

        /// <summary>
        /// 直接发布投稿键盘
        /// </summary>
        /// <param name="anymouse"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public InlineKeyboardMarkup DirectPostKeyboard(bool anymouse, BuildInTags tag)
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData( tag.HasFlag(BuildInTags.NSFW)? TagNSFWOn:TagNSFWOff, "review tag nsfw"),
                    InlineKeyboardButton.WithCallbackData(tag.HasFlag(BuildInTags.WanAn)? TagWanAnOn:TagWanAnOff, "review tag wanan"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(tag.HasFlag(BuildInTags.Friend)? TagFriendOn:TagFriendOff, "review tag friend"),
                    InlineKeyboardButton.WithCallbackData(tag.HasFlag(BuildInTags.AIGraph)? TagAIGraphOn:TagAIGraphOff, "review tag ai"),
                },

                new []
                {
                    InlineKeyboardButton.WithCallbackData( anymouse ? AnymouseOn : AnymouseOff, "review anymouse"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( PostCancel,  "review cancel"),
                    InlineKeyboardButton.WithCallbackData( ReviewAccept,  "review accept"),
                },
            });
            return keyboard;
        }

        /// <summary>
        /// 审核键盘A(选择稿件Tag)
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public InlineKeyboardMarkup ReviewKeyboardA(BuildInTags tag)
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData( tag.HasFlag(BuildInTags.NSFW)? TagNSFWOn:TagNSFWOff, "review tag nsfw"),
                    InlineKeyboardButton.WithCallbackData(tag.HasFlag(BuildInTags.WanAn)? TagWanAnOn:TagWanAnOff, "review tag wanan"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(tag.HasFlag(BuildInTags.Friend)? TagFriendOn:TagFriendOff, "review tag friend"),
                    InlineKeyboardButton.WithCallbackData(tag.HasFlag(BuildInTags.AIGraph)? TagAIGraphOn:TagAIGraphOff, "review tag ai"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( ReviewReject,  "review reject"),
                    InlineKeyboardButton.WithCallbackData( ReviewAccept,  "review accept"),
                },
            });
            return keyboard;
        }

        /// <summary>
        /// 审核键盘B(选择拒绝理由)
        /// </summary>
        /// <returns></returns>
        public InlineKeyboardMarkup ReviewKeyboardB()
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(RejectFuzzy, "reject fuzzy"),
                    InlineKeyboardButton.WithCallbackData(RejectDuplicate, "reject duplicate"),
                    InlineKeyboardButton.WithCallbackData(RejectBoring, "reject boring"),
                    InlineKeyboardButton.WithCallbackData(RejectConfusing, "reject confusing"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(RejectDeny, "reject deny"),
                    InlineKeyboardButton.WithCallbackData(RejectQRCode, "reject qrcode"),
                    InlineKeyboardButton.WithCallbackData(RejectOther, "reject other"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( RejectCancel,  "reject back"),
                },
            });
            return keyboard;
        }

        /// <summary>
        /// 设置用户群组键盘
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="targetUser"></param>
        /// <returns></returns>
        public async Task<InlineKeyboardMarkup?> SetUserGroupKeyboard(Users dbUser, Users targetUser)
        {
            var groups = await _groupRepository.Queryable().Where(x => x.Id > 0 && x.Id < dbUser.GroupID).ToListAsync();

            if (!groups.Any())
            {
                return null;
            }
            else
            {
                List<List<InlineKeyboardButton>> btns = new();

                foreach (var group in groups)
                {
                    var name = targetUser.GroupID == group.Id ? $"当前用户组: [ {group.Id}. {group.Name} ]" : $"{group.Id}. {group.Name}";
                    var data = $"cmd {dbUser.UserID} setusergroup {targetUser.UserID} {group.Id}";

                    btns.Add(new()
                    {
                        InlineKeyboardButton.WithCallbackData(name, data),
                    });
                }

                btns.Add(new()
                {
                        InlineKeyboardButton.WithCallbackData("取消操作", $"cmd {dbUser.UserID} cancel"),
                });

                InlineKeyboardMarkup keyboard = new(btns);

                return keyboard;
            }
        }

        /// <summary>
        /// 生成用户列表键盘
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="current">当前页码</param>
        /// <param name="total">总页码</param>
        /// <returns></returns>
        public InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total)
        {
            var btnClose = InlineKeyboardButton.WithCallbackData("关闭", $"cmd {dbUser.UserID} cancelclose 已关闭");

            if (total == 1)
            {
                InlineKeyboardMarkup keyboard = new(new[]
                {
                    new[]
                    {
                        btnClose,
                    },
                });

                return keyboard;
            }
            else
            {
                var btnPage = InlineKeyboardButton.WithCallbackData($"{current} / {total}", $"cmd {dbUser.UserID} say 当前 {current} 页, 共 {total} 页");

                var btnPrev = current > 1 ?
                    InlineKeyboardButton.WithCallbackData("上一页", $"cmd {dbUser.UserID} searchuser {query} {current - 1}") :
                    InlineKeyboardButton.WithCallbackData("到头了", $"cmd {dbUser.UserID} say 到头了");
                var btnNext = current < total ?
                    InlineKeyboardButton.WithCallbackData("下一页", $"cmd {dbUser.UserID} searchuser {query} {current + 1}") :
                    InlineKeyboardButton.WithCallbackData("到头了", $"cmd {dbUser.UserID} say 到头了");

                InlineKeyboardMarkup keyboard = new(new[]
                {
                    new[]
                    {
                        btnPrev, btnPage, btnNext,
                    },
                    new[]
                    {
                        btnClose,
                    },
                });

                return keyboard;
            }
        }
    }
}
