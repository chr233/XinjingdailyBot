using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Helper
{
    [AppService(typeof(IMarkupHelperService), LifeTime.Transient)]
    public sealed class MarkupHelperService : IMarkupHelperService
    {
        private readonly GroupRepository _groupRepository;
        private readonly IChannelService _channelService;
        private readonly TagRepository _tagRepository;
        private readonly RejectReasonRepository _rejectReasonRepository;

        public MarkupHelperService(
            GroupRepository groupRepository,
            IChannelService channelService,
            TagRepository tagRepository,
            RejectReasonRepository rejectReasonRepository)
        {
            _groupRepository = groupRepository;
            _channelService = channelService;
            _tagRepository = tagRepository;
            _rejectReasonRepository = rejectReasonRepository;
        }

        public InlineKeyboardMarkup PostKeyboard(bool anymouse)

        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(anymouse? Langs.AnymouseOn: Langs.AnymouseOff, "post anymouse"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(Langs.PostCancel, "post cancel"),
                    InlineKeyboardButton.WithCallbackData(Langs.PostConfirm, "post confirm"),
                },
            });
            return keyboard;
        }

        public InlineKeyboardMarkup DirectPostKeyboard(bool anymouse, int tagNum, bool? hasSpoiler)
        {
            var tags = _tagRepository.GetTagsPayload(tagNum);

            List<IEnumerable<InlineKeyboardButton>> btns = new();
            List<InlineKeyboardButton> line = new();

            int lineCount = tags.Count() <= 4 ? 2 : 3;

            foreach (var tag in tags)
            {
                line.Add(InlineKeyboardButton.WithCallbackData(tag.DisplayName, $"review tag {tag.Payload}"));
                if (line.Count >= lineCount)
                {
                    btns.Add(line);
                    line = new();
                }
            }

            if (line.Any())
            {
                btns.Add(line);
            }

            if (hasSpoiler.HasValue)
            {
                btns.Add(new[]
                {
                     InlineKeyboardButton.WithCallbackData(hasSpoiler.Value? Langs.TagSpoilerOn: Langs.TagSpoilerOff, "review spoiler"),
                     InlineKeyboardButton.WithCallbackData(anymouse? Langs.AnymouseOn: Langs.AnymouseOff, "review anymouse"),
                });
            }
            else
            {
                btns.Add(new[]
                {
                     InlineKeyboardButton.WithCallbackData(anymouse? Langs.AnymouseOn: Langs.AnymouseOff, "review anymouse"),
                });
            }
            btns.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(Langs.PostCancel, "review cancel"),
                InlineKeyboardButton.WithCallbackData(Langs.ReviewAccept, "review accept"),
            });

            return new(btns);
        }

        public InlineKeyboardMarkup ReviewKeyboardA(int tagNum, bool? hasSpoiler)
        {
            var tags = _tagRepository.GetTagsPayload(tagNum);

            int lineCount = tags.Count() <= 4 ? 2 : 3;

            var btns = new List<IEnumerable<InlineKeyboardButton>>();
            var line = new List<InlineKeyboardButton>();
            foreach (var tag in tags)
            {
                line.Add(InlineKeyboardButton.WithCallbackData(tag.DisplayName, $"review tag {tag.Payload}"));
                if (line.Count >= lineCount)
                {
                    btns.Add(line);
                    line = new List<InlineKeyboardButton>();
                }
            }

            if (line.Any())
            {
                btns.Add(line);
            }

            if (hasSpoiler.HasValue)
            {
                btns.Add(new[]
                {
                     InlineKeyboardButton.WithCallbackData(hasSpoiler.Value? Langs.TagSpoilerOn: Langs.TagSpoilerOff, "review spoiler"),
                });
            }

            btns.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(Langs.ReviewReject, "review reject"),
                InlineKeyboardButton.WithCallbackData(Langs.ReviewAccept, "review accept"),
            });

            return new InlineKeyboardMarkup(btns);
        }

        public InlineKeyboardMarkup ReviewKeyboardB()
        {
            var reasons = _rejectReasonRepository.GetAllRejectReasons();

            int lineCount = reasons.Count() <= 4 ? 2 : 3;

            var btns = new List<IEnumerable<InlineKeyboardButton>>();
            var line = new List<InlineKeyboardButton>();
            foreach (var reason in reasons)
            {
                line.Add(InlineKeyboardButton.WithCallbackData(reason.Name, $"review tag {reason.Payload}"));
                if (line.Count >= lineCount)
                {
                    btns.Add(line);
                    line = new List<InlineKeyboardButton>();
                }
            }

            btns.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(Langs.RejectCancel, "reject back"),
            });


            return new InlineKeyboardMarkup(btns);
        }

        public async Task<InlineKeyboardMarkup?> SetUserGroupKeyboard(Users dbUser, Users targetUser)
        {
            var groups = await _groupRepository.Queryable().Where(x => x.Id > 0 && x.Id < dbUser.GroupID).ToListAsync();

            if (!groups.Any())
            {
                return null;
            }
            else
            {
                List<IEnumerable<InlineKeyboardButton>> btns = new();
                List<InlineKeyboardButton> line = new();

                int lineCount = groups.Count <= 6 ? 2 : 3;

                foreach (var group in groups)
                {
                    var name = targetUser.GroupID == group.Id ? $"当前用户组: [ {group.Id}. {group.Name} ]" : $"{group.Id}. {group.Name}";
                    var data = $"cmd {dbUser.UserID} setusergroup {targetUser.UserID} {group.Id}";

                    line.Add(InlineKeyboardButton.WithCallbackData(name, data));
                    if (line.Count >= lineCount)
                    {
                        btns.Add(line);
                        line = new();
                    }
                }

                if (line.Any())
                {
                    btns.Add(line);
                }

                btns.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("取消操作", $"cmd {dbUser.UserID} cancel"),
                });

                InlineKeyboardMarkup keyboard = new(btns);

                return keyboard;
            }
        }

        public InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total)
        {
            var btnClose = InlineKeyboardButton.WithCallbackData("关闭", $"cmd {dbUser.UserID} cancelsearchuser 已关闭");

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

        public InlineKeyboardMarkup? SetChannelOptionKeyboard(Users dbUser, long channelId)
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "1. 不做特殊处理", $"cmd {dbUser.UserID} channeloption {channelId} normal"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "2. 抹除频道来源", $"cmd {dbUser.UserID} channeloption {channelId} purgeorigin"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "3. 拒绝此频道的投稿", $"cmd {dbUser.UserID} channeloption {channelId} autoreject"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "取消操作", $"cmd {dbUser.UserID} cancel"),
                }
            });

            return keyboard;
        }

        public InlineKeyboardMarkup? LinkToOriginPostKeyboard(NewPosts post)
        {
            var channel = _channelService.AcceptChannel;
            string link = channel.GetMessageLink(post.PublicMsgID);

            InlineKeyboardMarkup keyboard = new(new[]
             {
                new []
                {
                    InlineKeyboardButton.WithUrl($"在{channel.Title}中查看", link),
                },
            });
            return keyboard;
        }

        public InlineKeyboardMarkup? LinkToOriginPostKeyboard(long messageId)
        {
            var channel = _channelService.AcceptChannel;
            string link = channel.GetMessageLink(messageId);

            InlineKeyboardMarkup keyboard = new(new[]
             {
                new []
                {
                    InlineKeyboardButton.WithUrl($"在{channel.Title}中查看", link),
                },
            });
            return keyboard;
        }

        public InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser)
        {
            var tags = _tagRepository.GetAllTags();

            List<IEnumerable<InlineKeyboardButton>> btns = new()
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("不限制稿件标签", $"cmd {dbUser.UserID} setrandompost")
                }
            };
            List<InlineKeyboardButton> line = new();

            int lineCount = tags.Count() <= 4 ? 2 : 3;

            foreach (var tag in tags)
            {
                line.Add(InlineKeyboardButton.WithCallbackData($"{tag.OnText}", $"cmd {dbUser.UserID} setrandompost {tag.Payload}"));
                if (line.Count >= lineCount)
                {
                    btns.Add(line);
                    line = new();
                }
            }

            if (line.Any())
            {
                btns.Add(line);
            }

            btns.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("取消", $"cmd {dbUser.UserID} cancel"),
            });

            return new(btns);
        }

        public InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, int tagNum)
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "不限类型的随机稿件", $"cmd {dbUser.UserID} randompost {tagNum} all"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "随机图片", $"cmd {dbUser.UserID} randompost {tagNum} photo"),
                    InlineKeyboardButton.WithCallbackData( "随机视频", $"cmd {dbUser.UserID} randompost {tagNum} video"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "随机音频", $"cmd {dbUser.UserID} randompost {tagNum} audio"),
                    InlineKeyboardButton.WithCallbackData( "随机GIF", $"cmd {dbUser.UserID} randompost {tagNum} animation"),
                    InlineKeyboardButton.WithCallbackData( "随机文件", $"cmd {dbUser.UserID} randompost {tagNum} document"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "返回", $"cmd {dbUser.UserID} backrandompost"),
                    InlineKeyboardButton.WithCallbackData( "取消", $"cmd {dbUser.UserID} cancel"),
                }
            });

            return keyboard;
        }

        public InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, NewPosts post, int tagId, string postType)
        {
            var channel = _channelService.AcceptChannel;
            string link = channel.GetMessageLink(post.PublicMsgID);

            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithUrl($"在{channel.Title}中查看", link),
                    InlineKeyboardButton.WithCallbackData("再来一张",$"cmd {dbUser.UserID} randompost {tagId} {postType} {post.PublicMsgID}"),
                },
            });

            return keyboard;
        }

        public InlineKeyboardMarkup QueryPostMenuKeyboard(Users dbUser, NewPosts post)
        {
            InlineKeyboardMarkup keyboard;

            if (post.Status == EPostStatus.Accepted)
            {
                keyboard = new(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("撤回稿件",$"cmd {dbUser.UserID} deletepost {post.Id}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("查询投稿人成分",$"cmd {dbUser.UserID} queryposter {post.PosterUID}"),
                    },
                });
            }
            else
            {
                keyboard = new(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("补发稿件",$"cmd {dbUser.UserID} repost {post.Id}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("查询投稿人成分",$"cmd {dbUser.UserID} queryposter {post.PosterUID}"),
                    },
                });
            }

            return keyboard;
        }

        public InlineKeyboardMarkup NukeMenuKeyboard(Users dbUser, Users targetUser, string reason)
        {
            InlineKeyboardMarkup keyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("全局禁言", $"cmd {dbUser.UserID} nuke mute {targetUser.UserID} {reason}"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("全局封禁",$"cmd {dbUser.UserID} nuke ban {targetUser.UserID} {reason}"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("撤销全局禁言",$"cmd {dbUser.UserID} nuke unmute {targetUser.UserID} {reason}"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("撤销全局封禁",$"cmd {dbUser.UserID} nuke unban {targetUser.UserID} {reason}"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData( "取消操作", $"cmd {dbUser.UserID} cancel"),
                }
            });

            return keyboard;
        }
    }
}
