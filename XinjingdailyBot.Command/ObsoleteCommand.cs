using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Command;

/// <summary>
/// 超级管理员命令
/// </summary>
[Obsolete("迁移使用")]
//[AppService(LifeTime.Scoped)]
public sealed class ObsoleteCommand(
        ILogger<SuperCommand> _logger,
        ITelegramBotClient _botClient,
        IPostService _postService,
        OldPostRepository _oldPostService,
        IChannelOptionService _channelOptionService,
        IChannelService _channelService,
        IHttpHelperService _httpHelperService,
        ITextHelperService _textHelperService)
{
    /// <summary>
    /// 迁移旧的稿件数据
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("MERGEPOSTTAG", EUserRights.SuperCmd, Description = "迁移旧的稿件标签数据")]
    [Obsolete("迁移旧数据用")]
    public async Task ResponseMergePostTag(Message message)
    {
        const int threads = 30;

        int startId = 1;
        int effectCount = 0;

        int totalPosts = await _oldPostService.Queryable().CountAsync().ConfigureAwait(false);

        var msg = await _botClient.SendCommandReply($"开始更新稿件表, 共计 {totalPosts} 条记录", message, autoDelete: false).ConfigureAwait(false);

        while (startId <= totalPosts)
        {
            var oldOosts = await _oldPostService.Queryable().Where(x => x.Id >= startId && x.Tags != EBuildInTags.None).Take(threads).ToListAsync().ConfigureAwait(false);
            if (oldOosts.Count == 0)
            {
                break;
            }

            var tasks = oldOosts.Select(async oldPost => {
                if (oldPost.Tags != EBuildInTags.None)
                {
                    var oldTag = oldPost.Tags;
                    if (oldTag.HasFlag(EBuildInTags.Spoiler))
                    {
                        oldPost.HasSpoiler = true;
                    }
                    int newTag = 0;
                    if (oldTag.HasFlag(EBuildInTags.NSFW))
                    {
                        newTag += 1;
                    }
                    if (oldTag.HasFlag(EBuildInTags.Friend))
                    {
                        newTag += 2;
                    }
                    if (oldTag.HasFlag(EBuildInTags.WanAn))
                    {
                        newTag += 4;
                    }
                    if (oldTag.HasFlag(EBuildInTags.AIGraph))
                    {
                        newTag += 8;
                    }
                    oldPost.Tags = EBuildInTags.None;
                    oldPost.NewTags = newTag;
                    oldPost.ModifyAt = DateTime.Now;

                    effectCount++;

                    await _oldPostService.Updateable(oldPost).UpdateColumns(static x => new {
                        x.Tags,
                        x.NewTags,
                        x.ModifyAt
                    }).ExecuteCommandAsync().ConfigureAwait(false);
                }
            }).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            startId = oldOosts.Last().Id + 1;

            _logger.LogInformation("更新进度 {startId} / {totalUsers}, 更新数量 {effectCount}", startId, totalPosts, effectCount);
        }

        try
        {
            await _botClient.EditMessageTextAsync(msg, $"更新稿件表完成, 更新了 {effectCount} 条记录").ConfigureAwait(false);
        }
        catch
        {
            await _botClient.SendCommandReply($"更新稿件表完成, 更新了 {effectCount} 条记录", message, autoDelete: false).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 迁移旧的稿件数据
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("MERGEPOST", EUserRights.SuperCmd, Description = "迁移旧的稿件数据")]
    [Obsolete("迁移旧数据用")]
    public async Task ResponseMergePost(Message message)
    {
        const int threads = 30;

        int startId = 1;
        int effectCount = 0;

        int totalPosts = await _oldPostService.Queryable().CountAsync(x => !x.Merged).ConfigureAwait(false);
        var msg = await _botClient.SendCommandReply($"开始迁移稿件表, 共计 {totalPosts} 条记录", message, autoDelete: false).ConfigureAwait(false);

        while (startId <= totalPosts)
        {
            var oldPosts = await _oldPostService.Queryable().Where(x => x.Id >= startId && !x.Merged).Take(threads).ToListAsync().ConfigureAwait(false);
            if (oldPosts.Count == 0)
            {
                break;
            }

            var tasks = oldPosts.Select(async oldPost => {

                long channelId = -1, channelMsgId = -1;
                if (oldPost.IsFromChannel)
                {
                    ChannelOptions? channel = null;

                    var name = oldPost.ChannelName;
                    var title = oldPost.ChannelTitle;

                    if (name.EndsWith('~'))
                    {
                        name = name[..^1];
                    }

                    var text = name.Split('/');
                    if (text.Length >= 2)
                    {
                        if (!long.TryParse(text[1], out channelMsgId))
                        {
                            channelMsgId = -1;
                        }
                        channel = await _channelOptionService.FetchChannelByNameOrTitle(text[0], title).ConfigureAwait(false);
                    }
                    else
                    {
                        channel = await _channelOptionService.FetchChannelByNameOrTitle(name, title).ConfigureAwait(false);
                    }

                    if (channel != null)
                    {
                        channelId = channel.ChannelID;
                    }
                }

                string reason = oldPost.Reason switch {
                    ERejectReason.Fuzzy => "模糊",
                    ERejectReason.Duplicate => "重复",
                    ERejectReason.Boring => "无趣",
                    ERejectReason.Confused => "没懂",
                    ERejectReason.Deny => "内容不合适",
                    ERejectReason.QRCode => "广告水印",
                    ERejectReason.Other => "其他原因",
                    ERejectReason.CustomReason => "自定义拒绝理由",
                    ERejectReason.AutoReject => "稿件审核超时",
                    _ => "",
                };

                bool countReject = oldPost.Status == EPostStatus.Rejected && (oldPost.Reason != ERejectReason.Fuzzy && oldPost.Reason != ERejectReason.Duplicate);

                var post = new Posts {
                    Id = oldPost.Id,
                    OriginChatID = oldPost.OriginChatID,
                    OriginMsgID = oldPost.OriginMsgID,
                    OriginActionChatID = oldPost.OriginChatID,
                    OriginActionMsgID = oldPost.ActionMsgID,
                    PublicMsgID = oldPost.PublicMsgID,
                    Anonymous = oldPost.Anonymous,
                    Text = oldPost.Text,
                    RawText = oldPost.RawText,
                    ChannelID = channelId,
                    ChannelMsgID = channelMsgId,
                    Status = oldPost.Status,
                    PostType = oldPost.PostType,
                    OriginMediaGroupID = "",
                    ReviewMediaGroupID = "",
                    PublishMediaGroupID = "",
                    Tags = oldPost.NewTags,
                    HasSpoiler = oldPost.HasSpoiler,
                    RejectReason = reason,
                    CountReject = countReject,
                    PosterUID = oldPost.PosterUID,
                    ReviewerUID = oldPost.ReviewerUID,
                    CreateAt = oldPost.CreateAt,
                };

                if (oldPost.IsDirectPost)
                {
                    post.ReviewChatID = oldPost.OriginChatID;
                    post.ReviewMsgID = oldPost.OriginMsgID;
                    post.ReviewActionChatID = oldPost.OriginChatID;
                    post.ReviewActionMsgID = oldPost.ActionMsgID;
                }
                else
                {
                    post.ReviewChatID = _channelService.ReviewGroup.Id;
                    post.ReviewMsgID = oldPost.ReviewMsgID;
                    post.ReviewActionChatID = _channelService.ReviewGroup.Id;
                    post.ReviewActionMsgID = oldPost.ManageMsgID;
                }

                effectCount++;

                post.ModifyAt = DateTime.Now;

                try
                {
                    await _postService.Insertable(post).OffIdentity().ExecuteCommandAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    _logger.LogWarning("稿件Id {id} 已存在", oldPost.Id);
                    await _postService.Updateable(post).ExecuteCommandAsync().ConfigureAwait(false);
                }

                oldPost.Merged = true;
                await _oldPostService.Updateable(oldPost).UpdateColumns(static x => new { x.Merged }).ExecuteCommandAsync().ConfigureAwait(false);

            }).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            startId = oldPosts.Last().Id + 1;

            _logger.LogInformation("迁移进度 {startId} / {totalUsers}, 更新数量 {effectCount}", startId, totalPosts, effectCount);
        }

        try
        {
            await _botClient.EditMessageTextAsync(msg, $"迁移稿件表完成, 更新了 {effectCount} 条记录").ConfigureAwait(false);
        }
        catch
        {
            await _botClient.SendCommandReply($"迁移稿件表完成, 更新了 {effectCount} 条记录", message, autoDelete: false).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 修补稿件数据
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("FIXPOST", EUserRights.SuperCmd, Description = "修补稿件数据")]
    [Obsolete("过时方法")]
    public async Task ResponseFixPost(Message message)
    {
        const int threads = 30;

        int startId = 1;
        int effectCount = 0;

        int totalPosts = await _postService.Queryable().CountAsync(x => x.ReviewActionChatID == x.ReviewActionMsgID).ConfigureAwait(false);
        var msg = await _botClient.SendCommandReply($"开始修补稿件表, 共计 {totalPosts} 条记录", message, autoDelete: false).ConfigureAwait(false);

        while (startId <= totalPosts)
        {
            var posts = await _postService.Queryable().Where(x => x.Id >= startId &&
                x.ReviewActionChatID == x.ReviewActionMsgID
            ).Take(threads).ToListAsync().ConfigureAwait(false);

            if (posts.Count == 0)
            {
                break;
            }

            var tasks = posts.Select(async post => {
                effectCount++;

                post.ReviewActionChatID = post.ReviewChatID;
                post.ModifyAt = DateTime.Now;

                await _postService.Updateable(post).UpdateColumns(static x => new { x.ReviewActionChatID, x.ModifyAt }).ExecuteCommandAsync().ConfigureAwait(false);

            }).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            startId = posts.Last().Id + 1;

            _logger.LogInformation("迁移进度 {startId} / {totalUsers}, 更新数量 {effectCount}", startId, totalPosts, effectCount);
        }

        try
        {
            await _botClient.EditMessageTextAsync(msg, $"修补稿件表完成, 更新了 {effectCount} 条记录").ConfigureAwait(false);
        }
        catch
        {
            await _botClient.SendCommandReply($"修补稿件表完成, 更新了 {effectCount} 条记录", message, autoDelete: false).ConfigureAwait(false);
        }
    }
}
