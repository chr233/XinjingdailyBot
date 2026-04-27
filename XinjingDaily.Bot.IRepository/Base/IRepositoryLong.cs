namespace XinjingDaily.Bot.IRepository.Base;

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepositoryLong<TEntity> : IRepository<TEntity, long> where TEntity : class, new()
{
}
