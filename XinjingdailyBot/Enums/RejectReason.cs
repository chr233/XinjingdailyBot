using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XinjingdailyBot.Enums
{
    /// <summary>
    /// 拒稿原因
    /// </summary>
    internal enum RejectReason : byte
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 图片模糊
        /// </summary>
        Fuzzy,
        /// <summary>
        /// 重复稿件
        /// </summary>
        Duplicate,
        /// <summary>
        /// 无趣
        /// </summary>
        Boring,
        /// <summary>
        /// 内容不接受
        /// </summary>
        Deny,
        /// <summary>
        /// 审核超时,自动拒稿
        /// </summary>
        AutoReject,
        /// <summary>
        /// 已接受
        /// </summary>
        Accepted = byte.MaxValue
    }
}
