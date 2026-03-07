namespace XinjingDaily.Bot.Entry.Model;

/// <summary>
/// 分页数据
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Page"></param>
/// <param name="Count"></param>
/// <param name="TotalPage"></param>
/// <param name="TotalNumber"></param>
/// <param name="Data"></param>
public sealed record PageData<T>(int Page, int Count, int TotalPage, int TotalNumber, List<T> Data) where T : class
{
}

