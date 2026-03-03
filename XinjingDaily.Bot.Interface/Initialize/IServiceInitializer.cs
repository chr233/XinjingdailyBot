namespace XinjingDaily.Bot.Interface.InitService;

public interface IServiceInitializer
{
    /// <summary>
    /// 执行顺序, 数字越小越先执行
    /// </summary>
    int Order { get; }
    /// <summary>
    /// 初始化方法, 如果抛出异常则停止运行
    /// </summary>
    Task InitializeAsync();
}
