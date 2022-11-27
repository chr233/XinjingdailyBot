using System.Collections.Concurrent;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Parser;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages
{
    internal static class PostHandler
    {
        private static int MaxPostText { get; } = 2000;

        /// <summary>
        /// 获取频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelName"></param>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        private static async Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle)
        {
            var channel = await DB.Queryable<Channels>().Where(x => x.ChannelID == channelId).FirstAsync();
            if (channel == null)
            {
                channel = new()
                {
                    ChannelID = channelId,
                    ChannelName = channelName ?? "",
                    ChannelTitle = channelTitle ?? "",
                    Option = ChannelOption.Normal,
                    CreateAt = DateTime.Now,
                    ModifyAt = DateTime.Now,
                };
                await DB.Insertable(channel).ExecuteCommandAsync();
            }
            else if (channel.ChannelName != channelName || channel.ChannelTitle != channelTitle)
            {
                channel.ChannelTitle = channelTitle ?? "";
                channel.ChannelName = channelName ?? "";
                channel.ModifyAt = DateTime.Now;
                await DB.Updateable(channel).ExecuteCommandAsync();
            }

            return channel.Option;
        }

        /// <summary>
        /// 处理文字投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task HandleTextPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync(Langs.NoPostRight, message);
                return;
            }
            if (ReviewGroup.Id == -1)
            {
                await botClient.AutoReplyAsync(Langs.ReviewGroupNotSet, message);
                return;
            }

            if (string.IsNullOrEmpty(message.Text))
            {
                await botClient.AutoReplyAsync(Langs.TextPostCantBeNull, message);
                return;
            }

            if (message.Text!.Length > MaxPostText)
            {
                await botClient.AutoReplyAsync($"文本长度超过上限 {MaxPostText}, 无法创建投稿", message);
                return;
            }

            ChannelOption channelOption = ChannelOption.Normal;
            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                long channelId = message.ForwardFromChat.Id;
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                channelTitle = message.ForwardFromChat.Title;
                channelOption = await FetchChannelOption(channelId, channelName, channelTitle);
            }

            BuildInTags tags = TextHelper.FetchTags(message.Text);
            string text = MessageEntitiesParser.ParseMessage(message);

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            //生成数据库实体
            Posts newPost = new()
            {
                Anymouse = anymouse,
                Text = text,
                RawText = message.Text ?? "",
                ChannelName = channelName ?? "",
                ChannelTitle = channelTitle ?? "",
                Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
                PostType = message.Type,
                Tags = tags,
                PosterUID = dbUser.UserID
            };

            //套用频道设定
            switch (channelOption)
            {
                case ChannelOption.Normal:
                    break;
                case ChannelOption.PurgeOrigin:
                    postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                    newPost.ChannelName += '~';
                    break;
                case ChannelOption.AutoReject:
                    postText = "由于系统设定, 暂不接受来自此频道的投稿";
                    keyboard = null;
                    newPost.Status = PostStatus.Rejected;
                    break;
                default:
                    Logger.Error($"未知的频道选项 {channelOption}");
                    return;
            }

            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            //修改数据库实体
            newPost.OriginChatID = message.Chat.Id;
            newPost.OriginMsgID = message.MessageId;
            newPost.ActionMsgID = msg.MessageId;

            if (directPost)
            {
                newPost.ReviewMsgID = msg.MessageId;
                newPost.ManageMsgID = msg.MessageId;
            }

            await DB.Insertable(newPost).ExecuteCommandAsync();
        }

        /// <summary>
        /// 处理多媒体投稿
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task HandleMediaPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync("没有权限", message);
                return;
            }
            if (ReviewGroup.Id == -1)
            {
                await botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
                return;
            }

            ChannelOption channelOption = ChannelOption.Normal;
            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                long channelId = message.ForwardFromChat.Id;
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                channelTitle = message.ForwardFromChat.Title;
                channelOption = await FetchChannelOption(channelId, channelName, channelTitle);
            }

            BuildInTags tags = TextHelper.FetchTags(message.Caption);
            string text = MessageEntitiesParser.ParseMessage(message);

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

            //生成数据库实体
            Posts newPost = new()
            {
                Anymouse = anymouse,
                Text = text,
                RawText = message.Text ?? "",
                ChannelName = channelName ?? "",
                ChannelTitle = channelTitle ?? "",
                Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
                PostType = message.Type,
                Tags = tags,
                PosterUID = dbUser.UserID
            };

            //套用频道设定
            switch (channelOption)
            {
                case ChannelOption.Normal:
                    break;
                case ChannelOption.PurgeOrigin:
                    postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                    newPost.ChannelName += '~';
                    break;
                case ChannelOption.AutoReject:
                    postText = "由于系统设定, 暂不接受来自此频道的投稿";
                    keyboard = null;
                    newPost.Status = PostStatus.Rejected;
                    break;
                default:
                    Logger.Error($"未知的频道选项 {channelOption}");
                    return;
            }

            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            //修改数据库实体
            newPost.OriginChatID = message.Chat.Id;
            newPost.OriginMsgID = message.MessageId;
            newPost.ActionMsgID = msg.MessageId;

            if (directPost)
            {
                newPost.ReviewMsgID = msg.MessageId;
                newPost.ManageMsgID = msg.MessageId;
            }

            long postID = await DB.Insertable(newPost).ExecuteReturnBigIdentityAsync();

            Attachments? attachment = AttachmentHelpers.GenerateAttachment(message, postID);

            if (attachment != null)
            {
                await DB.Insertable(attachment).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// mediaGroupID字典
        /// </summary>
        private static ConcurrentDictionary<string, long> MediaGroupIDs { get; } = new();

        /// <summary>
        /// 处理多媒体投稿(mediaGroup)
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task HandleMediaGroupPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync("没有权限", message);
                return;
            }
            if (ReviewGroup.Id == -1)
            {
                await botClient.AutoReplyAsync("尚未设置投稿群组, 无法接收投稿", message);
                return;
            }

            string mediaGroupId = message.MediaGroupId!;
            if (!MediaGroupIDs.TryGetValue(mediaGroupId, out long postID)) //如果mediaGroupId不存在则创建新Post
            {
                MediaGroupIDs.TryAdd(mediaGroupId, -1);

                bool exists = await DB.Queryable<Posts>().AnyAsync(x => x.MediaGroupID == mediaGroupId);
                if (!exists)
                {
                    ChannelOption channelOption = ChannelOption.Normal;
                    string? channelName = null, channelTitle = null;
                    if (message.ForwardFromChat?.Type == ChatType.Channel)
                    {
                        long channelId = message.ForwardFromChat.Id;
                        channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                        channelTitle = message.ForwardFromChat.Title;
                        channelOption = await FetchChannelOption(channelId, channelName, channelTitle);
                    }

                    BuildInTags tags = TextHelper.FetchTags(message.Caption);
                    string text = MessageEntitiesParser.ParseMessage(message);

                    bool anymouse = dbUser.PreferAnymouse;

                    //直接发布模式
                    bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);

                    //发送确认消息
                    var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
                    string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";

                    Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                    //生成数据库实体
                    Posts newPost = new()
                    {
                        OriginChatID = message.Chat.Id,
                        OriginMsgID = message.MessageId,
                        ActionMsgID = msg.MessageId,
                        Anymouse = anymouse,
                        Text = text,
                        RawText = message.Text ?? "",
                        ChannelName = channelName ?? "",
                        ChannelTitle = channelTitle ?? "",
                        Status = directPost ? PostStatus.Reviewing : PostStatus.Padding,
                        PostType = message.Type,
                        MediaGroupID = mediaGroupId,
                        Tags = tags,
                        PosterUID = dbUser.UserID,
                    };

                    //套用频道设定
                    switch (channelOption)
                    {
                        case ChannelOption.Normal:
                            break;
                        case ChannelOption.PurgeOrigin:
                            postText += "\n由于系统设定, 来自该频道的投稿将不会显示来源";
                            newPost.ChannelName += '~';
                            break;
                        case ChannelOption.AutoReject:
                            postText = "由于系统设定, 暂不接受来自此频道的投稿";
                            keyboard = null;
                            newPost.Status = PostStatus.Rejected;
                            break;
                        default:
                            Logger.Error($"未知的频道选项 {channelOption}");
                            return;
                    }

                    if (directPost)
                    {
                        newPost.ReviewMsgID = msg.MessageId;
                        newPost.ManageMsgID = msg.MessageId;
                    }

                    postID = await DB.Insertable(newPost).ExecuteReturnBigIdentityAsync();

                    MediaGroupIDs[mediaGroupId] = postID;

                    //两秒后停止接收媒体组消息
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        MediaGroupIDs.Remove(mediaGroupId, out _);

                        await botClient.EditMessageTextAsync(msg, postText, replyMarkup: keyboard);
                    });
                }
            }

            //更新附件
            if (postID > 0)
            {
                Attachments? attachment = AttachmentHelpers.GenerateAttachment(message, postID);

                if (attachment != null)
                {
                    await DB.Insertable(attachment).ExecuteCommandAsync();
                }
            }
        }
    }
}
