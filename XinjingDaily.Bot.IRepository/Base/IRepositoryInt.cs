namespace XinjingDaily.Bot.IRepository.Base;

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepositoryInt<TEntity> : IRepository<TEntity, int> where TEntity : class, new()
{
}
