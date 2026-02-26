using SqlSugar;

namespace XinjingDaily.Bot.Repository.Base;

/// <summary>
/// 仓储基类
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public abstract class RepositoryInt<TEntity>(
    ISqlSugarClient db) : Repository<TEntity, int>(db) where TEntity : class, new()
{
}
