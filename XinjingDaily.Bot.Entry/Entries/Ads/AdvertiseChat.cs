using SqlSugar;
namespace XinjingDaily.Bot.Entry.Entries.Ads;

[SugarTable("advertise_channel", TableDescription = "广告投放位置")]
public sealed record AdvertiseChat
{
    /// <summary>
    /// Advertise主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int AdId { get; set; }

    /// <summary>
    /// ChatInfo主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public int ChatId { get; set; }
}
