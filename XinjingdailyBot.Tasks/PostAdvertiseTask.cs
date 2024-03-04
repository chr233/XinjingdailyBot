using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 发布广告
/// </summary>
[Schedule("0 0 10 * * ?")]
public sealed class PostAdvertiseTask(
        ILogger<PostAdvertiseTask> _logger,
        IServiceProvider _serviceProvider,
        ITelegramBotClient _botClient,
        IAdvertiseService _advertisesService,
        IAdvertisePostService _advertisePostService,
        IMarkupHelperService _markupHelperService) : IJob
{
    /// <inheritdoc/>
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 发布广告");

        var ad = await _advertisesService.GetPostableAdvertise().ConfigureAwait(false);

        if (ad == null)
        {
            return;
        }

        //取消置顶旧的广告
        await _advertisePostService.UnPinOldAdPosts(ad).ConfigureAwait(false);

        var channelService = _serviceProvider.GetRequiredService<IChannelService>();

        var operates = new List<(EAdMode, ChatId)>
        {
            (EAdMode.AcceptChannel, channelService.AcceptChannel),
            (EAdMode.RejectChannel, channelService.RejectChannel),
            //(EAdMode.ReviewGroup, channelService.ReviewGroup),
            (EAdMode.CommentGroup, channelService.CommentGroup),
            (EAdMode.SubGroup, channelService.SubGroup),
        };

        if (channelService.HasSecondChannel)
        {
            operates.Add((EAdMode.SecondChannel, channelService.SecondChannel!));
        }

        foreach (var (mode, chat) in operates)
        {
            if (ad.Mode.HasFlag(mode) && chat.Identifier != null)
            {
                var chatId = chat.Identifier.Value;

                try
                {
                    var msgId = await _botClient.CopyMessageAsync(chatId, ad.ChatID, (int)ad.MessageID, disableNotification: true).ConfigureAwait(false);

                    var kbd = _markupHelperService.AdvertiseExternalLinkButton(ad.ExternalLink, ad.ExternalLinkName);
                    if (kbd != null)
                    {
                        await _botClient.EditMessageReplyMarkupAsync(chat, msgId.Id, kbd).ConfigureAwait(false);
                    }

                    await _advertisePostService.AddAdPost(ad, chatId, msgId.Id).ConfigureAwait(false);

                    ad.ShowCount++;
                    ad.LastPostAt = DateTime.Now;

                    if (ad.PinMessage)
                    {
                        await _botClient.PinChatMessageAsync(chatId, msgId.Id, true).ConfigureAwait(false);
                    }

                    await _advertisesService.UpdateAdvertiseStatistics(ad).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "投放广告出错");
                }
                finally
                {
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }
        }
    }
}
