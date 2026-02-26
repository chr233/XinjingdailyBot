using SqlSugar;
using Telegram.Bot.Types.Enums;
namespace XinjingDaily.Bot.Entry.Entries.Ads;

[SugarTable("advertise_channel", TableDescription = "广告投放位置")]
public sealed record AdvertiseChannels
{
    /// <summary>
    /// Advertise主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int AdId { get; set; }

    [SugarColumn(IsPrimaryKey = true)]
    public int ChatId { get; set; }

    public ChatType Type { get; set; }
}
