using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Bot.Common;

/// <summary>
/// 频道和群组信息服务
/// </summary>
public interface IChannelService
{
    /// <summary>
    /// 审核群组
    /// </summary>
    Chat ReviewGroup { get; }
    /// <summary>
    /// 审核日志频道
    /// </summary>
    Chat? LogChannel { get; }
    /// <summary>
    /// 评论区群组
    /// </summary>
    Chat CommentGroup { get; }
    /// <summary>
    /// 闲聊区群组
    /// </summary>
    Chat SubGroup { get; }
    /// <summary>
    /// 发布频道
    /// </summary>
    Chat AcceptChannel { get; }
    /// <summary>
    /// 第二频道
    /// </summary>
    Chat? SecondChannel { get; }
    /// <summary>
    /// 第二频道评论区
    /// </summary>
    Chat? SecondCommentGroup { get; }
    /// <summary>
    /// 拒绝频道
    /// </summary>
    Chat RejectChannel { get; }
    /// <summary>
    /// 管理日志频道
    /// 用于存储封禁/解封日志
    /// </summary>
    Chat AdminLogChannel { get;  }
    /// <summary>
    /// 机器人用户
    /// </summary>
    User BotUser { get; }
    /// <summary>
    /// 是否设置第二频道
    /// </summary>
    bool HasSecondChannel { get; }

    /// <summary>
    /// 读取频道信息
    /// </summary>
    /// <returns></returns>
    Task InitChannelInfo();
    /// <summary>
    /// 判断是不是频道的消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <returns></returns>
    bool IsChannelMessage(long chatId);
    /// <summary>
    /// 判断是不是频道的消息
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    bool IsChannelMessage(Chat chat);

    /// <summary>
    /// 判断是不是关联群组的消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <returns></returns>
    bool IsGroupMessage(long chatId);
    /// <summary>
    /// 判断是不是关联群组的消息
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    bool IsGroupMessage(Chat chat);
    /// <summary>
    /// 判断是否是审核群组的信息
    /// </summary>
    /// <param name="chatId"></param>
    /// <returns></returns>
    bool IsReviewMessage(long chatId);
    /// <summary>
    /// 判断是否是审核群组的信息
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    bool IsReviewMessage(Chat chat);
    /// <summary>
    /// 更新频道信息
    /// </summary>
    /// <param name="chat"></param>
    /// <param name="newChatTitle"></param>
    void OnChatTitleChanged(Chat chat, string? newChatTitle);
}
