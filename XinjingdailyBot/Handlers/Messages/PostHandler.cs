using System.Text.RegularExpressions;
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
        public static string PureText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            text = Regex.Replace(text, @"(^#\S+)|(\s#\S+)", "");

            string[] parts = text.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            return string.Join('\n', parts);
        }

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

        public static async Task HandleTextPosts(ITelegramBotClient botClient, Users dbUser, Message message)
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

            BuildInTags tags = FetchTags(message.Text);
            string text = PureText(message.Text);

            bool anymouse = dbUser.PerferAnymouse;

            var keyboard = MarkupHelper.PostKeyboard(anymouse);
            Message msg = await botClient.SendTextMessageAsync(message.Chat.Id, "真的要投稿吗?", replyToMessageId: message.MessageId, replyMarkup: keyboard);

            //存入数据库
            Posts post = new()
            {
                OriginChatID = message.Chat.Id,
                OriginMsgID = message.MessageId,
                ActionMsgID = msg.MessageId,
                Anymouse = anymouse,
                Text = text,
                RawText = message.Text ?? "",
                ChannelName = channelName ?? "",
                ChannelTitle = channelTitle ?? "",
                Status = PostStatus.Padding,
                PostType = message.Type,
                Tags = tags,
                PosterUID = dbUser.UserID
            };

            await DB.Insertable(post).ExecuteCommandAsync();
        }

        private static Attachments? GenerateAttachment(Message message)
        {
            string? fileID, fileName, FileUid, mimeType;
            int size, height, width;

            switch (message.Type)
            {
                case MessageType.Photo:
                    {
                        var x = message.Photo!.Last();
                        fileID = x.FileId;
                        fileName = "";
                        FileUid = x.FileUniqueId;
                        mimeType = "";
                        size = x.FileSize ?? 0;
                        height = x.Height;
                        width = x.Width;
                    }
                    break;
                case MessageType.Audio:
                    {
                        var x = message.Audio!;
                        fileID = x.FileId;
                        fileName = x.FileName ?? "";
                        FileUid = x.FileUniqueId;
                        mimeType = x.MimeType ?? "";
                        size = x.FileSize ?? 0;
                        height = -1;
                        width = -1;
                    }
                    break;

                case MessageType.Video:
                    {
                        var x = message.Video!;
                        fileID = x.FileId;
                        fileName = x.FileName ?? "";
                        FileUid = x.FileUniqueId;
                        mimeType = x.MimeType ?? "";
                        size = x.FileSize ?? 0;
                        height = x.Height;
                        width = x.Width;
                    }
                    break;
                case MessageType.Document:
                    {
                        var x = message.Document!;
                        fileID = x.FileId;
                        fileName = x.FileName ?? "";
                        FileUid = x.FileUniqueId;
                        mimeType = x.MimeType ?? "";
                        size = x.FileSize ?? 0;
                        height = -1;
                        width = -1;
                    }
                    break;

                default:
                    return null;
            }

            Attachments result = new()
            {
                MediaGroupID = message.MediaGroupId ?? "",
                FileID = fileID,
                FileName = fileName,
                FileUniqueID = FileUid,
                MimeType = mimeType,
                Size = size,
                Height = height,
                Width = width,
            };

            return result;
        }


        public static async Task HandleMediaPosts(ITelegramBotClient botClient, Users dbUser, Message message)
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

            BuildInTags tags = FetchTags(message.Caption);
            string text = PureText(message.Caption);

            //存入数据库
            Posts textPost = new()
            {
                OriginChatID = message.Chat.Id,
                OriginMsgID = message.MessageId,
                Anymouse = dbUser.PerferAnymouse,
                Text = text,
                RawText = message.Text ?? "",
                ChannelName = channelName ?? "",
                ChannelTitle = channelTitle ?? "",
                Status = PostStatus.Padding,
                PostType = message.Type,
                Tags = tags,
                PosterUID = dbUser.UserID
            };

            long postID = await DB.Insertable(textPost).ExecuteReturnBigIdentityAsync();

            Attachments? attachment = GenerateAttachment(message);

            if (attachment != null)
            {
                await DB.Insertable(attachment).ExecuteCommandAsync();
            }

            //发送确认消息

            await botClient.AutoReplyAsync("rwr", message);
        }

        private static HashSet<string> MediaGroupIDs = new();

        public static async Task HandleMediaGroupPosts(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.SendPost))
            {
                await botClient.AutoReplyAsync("没有权限", message);
            }

            string mediaGroupId = message.MediaGroupId!;
            if (!MediaGroupIDs.Contains(mediaGroupId)) //不存在标记则创建标记
            {
                MediaGroupIDs.Add(mediaGroupId);

                bool exists = await DB.Queryable<Attachments>().AnyAsync(x => x.MediaGroupID == mediaGroupId);
                if (!exists)
                {
                    string? channelName = null, channelTitle = null;
                    if (message.ForwardFromChat?.Type == ChatType.Channel)
                    {
                        channelName = message.ForwardFromChat.Username;
                        channelTitle = message.ForwardFromChat.Title;
                    }

                    BuildInTags tags = FetchTags(message.Caption);
                    string text = PureText(message.Caption);

                    //存入数据库
                    Posts textPost = new()
                    {
                        OriginChatID = message.Chat.Id,
                        OriginMsgID = message.MessageId,
                        Anymouse = dbUser.PerferAnymouse,
                        Text = text,
                        RawText = message.Text ?? "",
                        ChannelName = channelName ?? "",
                        ChannelTitle = channelTitle ?? "",
                        Status = PostStatus.Padding,
                        PostType = message.Type,
                        MediaGroupID = mediaGroupId,
                        Tags = tags,
                        PosterUID = dbUser.UserID,
                    };

                    long postID = await DB.Insertable(textPost).ExecuteReturnBigIdentityAsync();

                    //两秒后发送确认消息
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        MediaGroupIDs.Remove(mediaGroupId);

                        int x = await DB.Queryable<Attachments>().CountAsync(x => x.MediaGroupID == mediaGroupId);

                        await botClient.AutoReplyAsync(x.ToString(), message);

                    });
                }
            }


            Attachments? attachment = GenerateAttachment(message);

            if (attachment != null)
            {
                await DB.Insertable(attachment).ExecuteCommandAsync();
            }
        }
    }
}
