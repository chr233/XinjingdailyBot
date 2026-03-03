using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Policys;
using XinjingDaily.Bot.IRepository.Policys;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Policys;

/// <summary>
/// 来源频道策略仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class SourceChannelPolicysRepository : RepositoryInt<SourceChannelPolicy>, ISourceChannelPolicyRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public SourceChannelPolicysRepository(ISqlSugarClient db) : base(db)
    {
    }
}
