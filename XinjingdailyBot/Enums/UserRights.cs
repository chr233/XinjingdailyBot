using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XinjingdailyBot.Enums
{
    [Flags]
    internal enum UserRights : byte
    {
        /// <summary>
        /// 投稿
        /// </summary>
        SendPost = 0x01,
        /// <summary>
        /// 审核
        /// </summary>
        ReviewPost = 0x02,
        /// <summary>
        /// 直接投稿
        /// </summary>
        DirectPost = 0x04,

        /// <summary>
        /// 普通命令
        /// </summary>
        NormalCmd = 0x10,
        /// <summary>
        /// 管理命令
        /// </summary>
        AdminCmd = 0x20,
        /// <summary>
        /// 超管命令
        /// </summary>
        SuperCmd = 0x40,
    }
}
