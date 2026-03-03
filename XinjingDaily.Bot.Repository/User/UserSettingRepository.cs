using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User;

/// <summary>
/// 用户设置仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class UserSettingRepository(ISqlSugarClient db) : RepositoryInt<UserSetting>(db), IUserSettingRepository
{
}
 