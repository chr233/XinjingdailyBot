using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler;

/// <summary>
/// 命令处理器
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// 生成可用命令信息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <returns></returns>
    string GetAvilabeCommands(Users dbUser);
    /// <summary>
    /// 设置菜命令单
    /// </summary>
    /// <returns></returns>
    Task<bool> SetCommandsMenu();
    /// <summary>
    /// 注册命令
    /// </summary>
    [RequiresUnreferencedCode("不兼容剪裁")]
    void InstallCommands();
    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task OnCommandReceived(Users dbUser, Message message);
    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    Task OnQueryCommandReceived(Users dbUser, CallbackQuery query);
    Task<bool> ClearCommandsMenu();
}
