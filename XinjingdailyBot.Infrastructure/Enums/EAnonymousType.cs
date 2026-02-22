namespace XinjingdailyBot.Infrastructure.Enums;

public enum EAnonymousType : byte
{
    /// <summary>
    /// 不匿名
    /// </summary>
    NotAnonymous = 0,
    /// <summary>
    /// 主动匿名
    /// </summary>
    Anonymous = 1,
    /// <summary>
    /// 强制匿名
    /// </summary>
    ForceAnonymous = 2,
}
