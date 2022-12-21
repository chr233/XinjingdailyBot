using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Enums;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Helper
{
    [AppService(ServiceType = typeof(IAttachmentHelperService), ServiceLifetime = LifeTime.Transient)]
    public sealed class AttachmentHelperService : IAttachmentHelperService
    {
        /// <summary>
        /// 附件包装器
        /// </summary>
        /// <param name="message"></param>
        /// <param name="postID"></param>
        /// <returns></returns>
        public Attachments? GenerateAttachment(Message message, long postID)
        {
            string? fileID, fileName, FileUid, mimeType;
            long size;
            int height, width;

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
                        fileName = x.Title ?? x.FileName ?? "";
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
                PostID = postID,
                FileID = fileID,
                FileName = fileName,
                FileUniqueID = FileUid,
                MimeType = mimeType,
                Size = size,
                Height = height,
                Width = width,
                Type = message.Type,
            };

            return result;
        }
    }
}
