using System;
using System.Collections.Generic;
using System.Text;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.Command;

namespace XinjingDaily.Bot.Service.Bot.Storage;


/// <summary>
/// 消息上下文服务
/// </summary>
public sealed class ContextService (
    ICommandContextRepository _commandContextRepository)
{
    public async Task CancelContext (UserInfo userInfo)
    {

    }
}
