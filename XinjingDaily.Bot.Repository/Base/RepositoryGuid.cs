using SqlSugar;

namespace XinjingDaily.Bot.Repository.Base;

/// <summary>
/// 仓储基类
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public abstract class RepositoryGuid<TEntity>(
    ISqlSugarClient db) : Repository<TEntity, Guid>(db) where TEntity : class, new()
{
}
