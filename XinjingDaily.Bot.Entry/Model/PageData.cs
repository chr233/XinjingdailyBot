namespace XinjingDaily.Bot.Entry.Model;

public sealed record PageData<T>(int Page, int Count, int TotalPage, int TotalNumber, List<T> Data) where T : class
{
}

