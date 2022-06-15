using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages
{
    internal sealed class PostHandler
    {
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
                await botClient.AutoReplyAsync("没有权限", message);
            }

            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                channelName = message.ForwardFromChat.Username;
                channelTitle = message.ForwardFromChat.Title;
            }

            BuildInTags tags = TextHelper.FetchTags(message.Text);
            string text = TextHelper.PureText(message.Text);

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "真的要投稿吗?", replyToMessageId: message.MessageId, replyMarkup: keyboard);

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
            }

            string? channelName = null, channelTitle = null;
            if (message.ForwardFromChat?.Type == ChatType.Channel)
            {
                channelName = message.ForwardFromChat.Username;
                channelTitle = message.ForwardFromChat.Title;
            }

            BuildInTags tags = TextHelper.FetchTags(message.Caption);
            string text = TextHelper.PureText(message.Caption);

            bool anymouse = dbUser.PreferAnymouse;

            //直接发布模式
            bool directPost = dbUser.Right.HasFlag(UserRights.DirectPost);
            //发送确认消息
            var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "真的要投稿吗?", replyToMessageId: message.MessageId, replyMarkup: keyboard);

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
                        channelName = message.ForwardFromChat.Username;
                        channelTitle = message.ForwardFromChat.Title;
                    }

                    BuildInTags tags = TextHelper.FetchTags(message.Caption);
                    string text = TextHelper.PureText(message.Caption);

                    bool anymouse = dbUser.PreferAnymouse;

                    Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "处理中", replyToMessageId: message.MessageId);

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
                        await Task.Delay(2000);
                        MediaGroupIDs.Remove(mediaGroupId, out _);

                        //发送确认消息
                        var keyboard = directPost ? MarkupHelper.DirectPostKeyboard(anymouse, tags) : MarkupHelper.PostKeyboard(anymouse);
                        await botClient.EditMessageTextAsync(msg, "真的要投稿吗", replyMarkup: keyboard);
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
