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

            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                channelTitle = message.ForwardFromChat.Title;
            }

            BuildInTags tags = TextHelper.FetchTags(message.Text);
            string text = MessageEntitiesParser.ParseMessage(message);

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";
            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            //存入数据库
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
                Tags = tags,
                PosterUID = dbUser.UserID
            };

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

            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                channelTitle = message.ForwardFromChat.Title;
            }

            BuildInTags tags = TextHelper.FetchTags(message.Caption);
            string text = MessageEntitiesParser.ParseMessage(message);

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
            string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";
            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, postText, replyToMessageId: message.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

            //存入数据库
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
                Tags = tags,
                PosterUID = dbUser.UserID
            };

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
                    string? channelName = null, channelTitle = null;
                    if (message.ForwardFromChat?.Type == ChatType.Channel)
                    {
                        channelName = $"{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}";
                        channelTitle = message.ForwardFromChat.Title;
                    }

                    BuildInTags tags = TextHelper.FetchTags(message.Caption);
                    string text = MessageEntitiesParser.ParseMessage(message);

                    bool anymouse = dbUser.PreferAnymouse;

                    Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "处理中, 请稍后", replyToMessageId: message.MessageId, allowSendingWithoutReply: true);

                    //直接发布模式
                    bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);

                    //存入数据库
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

                        //发送确认消息
                        var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
                        string postText = directPost ? "您具有直接投稿权限, 您的稿件将会直接发布" : "真的要投稿吗";
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
