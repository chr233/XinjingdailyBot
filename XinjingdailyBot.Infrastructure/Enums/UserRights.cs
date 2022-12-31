namespace XinjingdailyBot.Infrastructure.Enums
{
    /// <summary>
    /// 用户权限
    /// </summary>
    [Flags]
    public enum UserRights : byte
    {
        /// <summary>
        /// 无权限
        /// </summary>
        None = 0,

        /// <summary>
        /// 投稿
        /// </summary>
        SendPost = 1 << 0,

        /// <summary>
        /// 审核
        /// </summary>
        ReviewPost = 1 << 1,

        /// <summary>
        /// 直接投稿
        /// </summary>
        DirectPost = 1 << 2,

        /// <summary>
        /// 普通命令
        /// </summary>
        NormalCmd = 1 << 4,

        /// <summary>
        /// 管理命令
        /// </summary>
        AdminCmd = 1 << 5,

        /// <summary>
        /// 超管命令
        /// </summary>
        SuperCmd = 1 << 6,

        /// <summary>
        /// 火星
        /// </summary>
        Mars = 1 << 7,

        /// <summary>
        /// 普通用户
        /// </summary>
        NormalUser = SendPost | NormalCmd,

        /// <summary>
        /// 审核员
        /// </summary>
        Reviewer = NormalUser | ReviewPost,

        /// <summary>
        /// 发布员
        /// </summary>
        Poster = NormalUser | DirectPost,

        /// <summary>
        /// 火星救员
        /// </summary>
        TheMartian = NormalUser | Mars,

        /// <summary>
        /// 普通管理
        /// </summary>
        Admin = SendPost | ReviewPost | DirectPost | NormalCmd | AdminCmd | Mars,

        /// <summary>
        /// 超级管理
        /// </summary>
        SuperAdmin = Admin | SuperCmd,
    }
}
