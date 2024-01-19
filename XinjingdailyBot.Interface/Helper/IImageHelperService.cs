using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Helper;

/// <summary>
/// 图片处理服务
/// </summary>
public interface IImageHelperService
{
    Task<bool> ProcessMessage(Message msg);
}
