using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 投稿服务
/// </summary>
public interface IPostService : IBaseService<Posts>
{
    /// <summary>
    /// 收到媒体组第一条消息后过多久停止接收该媒体组并进行后续投稿操作, 单位ms
    /// </summary>
    public const double MediaGroupReceiveTtl = 1.5f;
    /// <summary>
    /// 文字投稿长度上限
    /// </summary>
    public const int MaxPostText = 2000;

    /// <summary>
    /// 接受投稿
    /// </summary>
    /// <param name="post"></param>
    /// <param name="dbUser"></param>
    /// <param name="inPlan"></param>
    /// <param name="second"></param>
    /// <param name="callbackQuery"></param>
    /// <returns></returns>
    Task AcceptPost(Posts post, Users dbUser, bool inPlan, bool second, CallbackQuery callbackQuery);
    /// <summary>
    /// 取消投稿
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    Task CancelPost(Posts post);

    /// <summary>
    /// 检查用户是否达到每日投稿上限
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <param name="query"></param>
    /// <returns>true: 可以继续投稿 false: 无法继续投稿</returns>
    Task<bool> CheckPostLimit(Users dbUser, Message? message = null, CallbackQuery? query = null);
    /// <summary>
    /// 统计通过投稿
    /// </summary>
    /// <param name="afterTime"></param>
    /// <returns></returns>
    Task<int> CountAcceptedPosts(DateTime afterTime);
    /// <summary>
    /// 统计通过投稿
    /// </summary>
    /// <returns></returns>
    Task<int> CountAcceptedPosts();
    /// <summary>
    /// 统计二频通过投稿
    /// </summary>
    /// <param name="afterTime"></param>
    /// <returns></returns>
    Task<int> CountAcceptedSecondPosts(DateTime afterTime);
    /// <summary>
    /// 统计二频通过投稿
    /// </summary>
    /// <returns></returns>
    Task<int> CountAcceptedSecondPosts();
    /// <summary>
    /// 统计全部投稿
    /// </summary>
    /// <param name="afterTime"></param>
    /// <returns></returns>
    Task<int> CountAllPosts(DateTime afterTime);
    /// <summary>
    /// 统计全部投稿
    /// </summary>
    /// <returns></returns>
    Task<int> CountAllPosts();
    /// <summary>
    /// 统计过期投稿
    /// </summary>
    /// <param name="afterTime"></param>
    /// <returns></returns>
    Task<int> CountExpiredPosts(DateTime afterTime);
    /// <summary>
    /// 统计过期投稿
    /// </summary>
    /// <returns></returns>
    Task<int> CountExpiredPosts();
    /// <summary>
    /// 统计拒绝投稿
    /// </summary>
    /// <param name="afterTime"></param>
    /// <returns></returns>
    Task<int> CountRejectedPosts(DateTime afterTime);
    /// <summary>
    /// 统计拒绝投稿
    /// </summary>
    /// <returns></returns>
    Task<int> CountRejectedPosts();
    /// <summary>
    /// 获取待审核稿件
    /// </summary>
    /// <param name="afterTime"></param>
    /// <returns></returns>
    Task<int> CountReviewingPosts(DateTime afterTime);

    /// <summary>
    /// 编辑稿件描述
    /// </summary>
    /// <param name="post"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    Task EditPostText(Posts post, string text);
    /// <summary>
    /// 是否存在指定媒体组ID的稿件
    /// </summary>
    /// <param name="mediaGroupId"></param>
    /// <returns></returns>
    Task<bool> IfExistsMediaGroupId(string mediaGroupId);

    /// <summary>
    /// 从审核回调中获取稿件
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<Posts?> FetchPostFromCallbackQuery(CallbackQuery message);
    /// <summary>
    /// 从回复的消息获取稿件
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<Posts?> FetchPostFromReplyToMessage(Message message);
    /// <summary>
    /// 获取计划发布的投稿
    /// </summary>
    /// <returns></returns>
    Task<Posts> GetInPlanPost();

    /// <summary>
    /// 获取最新未审核稿件
    /// </summary>
    /// <returns></returns>
    Task<Posts?> GetLatestReviewingPostLink();
    /// <summary>
    /// 获取稿件
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    Task<Posts?> GetPostByPostId(int postId);
    /// <summary>
    /// 获取随机稿件
    /// </summary>
    /// <returns></returns>
    Task<Posts?> GetRandomPost();

    /// <summary>
    /// 处理多媒体投稿(mediaGroup)
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task HandleMediaGroupPosts(Users dbUser, Message message);
    /// <summary>
    /// 处理多媒体投稿
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task HandleMediaPosts(Users dbUser, Message message);
    /// <summary>
    /// 处理文字投稿
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task HandleTextPosts(Users dbUser, Message message);
    /// <summary>
    /// 发布延时稿件
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    Task<bool> PublicInPlanPost(Posts post);

    /// <summary>
    /// 拒绝投稿
    /// </summary>
    /// <param name="post"></param>
    /// <param name="dbUser"></param>
    /// <param name="rejectReason"></param>
    /// <param name="htmlRejectMessage"></param>
    /// <returns></returns>
    Task RejectPost(Posts post, Users dbUser, RejectReasons rejectReason, string? htmlRejectMessage);
    /// <summary>
    /// 撤回稿件
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    Task RevocationPost(Posts post);
    /// <summary>
    /// 设置稿件匿名
    /// </summary>
    /// <param name="post"></param>
    /// <param name="anonymous"></param>
    /// <returns></returns>
    Task SetPostAnonymous(Posts post, bool anonymous);
    /// <summary>
    /// 设置稿件强制匿名
    /// </summary>
    /// <param name="post"></param>
    /// <param name="anonymous"></param>
    /// <returns></returns>
    Task SetPostForceAnonymous(Posts post, bool anonymous);
    /// <summary>
    /// 设置稿件遮罩
    /// </summary>
    /// <param name="post"></param>
    /// <param name="spoiler"></param>
    /// <returns></returns>
    Task SetPostSpoiler(Posts post, bool spoiler);

    /// <summary>
    /// 设置稿件Tag
    /// </summary>
    /// <param name="post"></param>
    /// <param name="tagId"></param>
    /// <param name="callbackQuery"></param>
    /// <returns></returns>
    Task SetPostTag(Posts post, int tagId, CallbackQuery callbackQuery);
    /// <summary>
    /// 设置稿件Tag
    /// </summary>
    /// <param name="post"></param>
    /// <param name="payload"></param>
    /// <param name="callbackQuery"></param>
    /// <returns></returns>
    Task SetPostTag(Posts post, string payload, CallbackQuery callbackQuery);
    /// <summary>
    /// 统计全部稿件
    /// </summary>
    /// <param name="afterTime"></param>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<int> CountAllPosts(DateTime afterTime, DateTime beforeTime);
    /// <summary>
    /// 统计二频通过稿件
    /// </summary>
    /// <param name="afterTime"></param>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<int> CountAcceptedSecondPosts(DateTime afterTime, DateTime beforeTime);
    /// <summary>
    /// 统计拒绝稿件
    /// </summary>
    /// <param name="afterTime"></param>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<int> CountRejectedPosts(DateTime afterTime, DateTime beforeTime);
    /// <summary>
    /// 统计审核中稿件
    /// </summary>
    /// <param name="afterTime"></param>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<int> CountReviewingPosts(DateTime afterTime, DateTime beforeTime);
    /// <summary>
    /// 统计通过稿件
    /// </summary>
    /// <param name="afterTime"></param>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<int> CountAcceptedPosts(DateTime afterTime, DateTime beforeTime);
    /// <summary>
    /// 更新稿件状态
    /// </summary>
    /// <param name="post"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    Task UpdatePostStatus(Posts post, EPostStatus status);
    /// <summary>
    /// 创建稿件
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    Task<int> CreateNewPosts(Posts post);
    /// <summary>
    /// 获取过期稿件列表
    /// </summary>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<List<Posts>> GetExpiredPosts(DateTime beforeTime);
    /// <summary>
    /// 获取过期稿件列表
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="beforeTime"></param>
    /// <returns></returns>
    Task<List<Posts>> GetExpiredPosts(long userID, DateTime beforeTime);
    void InitTtlTimer();
}
