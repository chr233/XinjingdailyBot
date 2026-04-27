namespace XinjingDaily.Bot.IRepository.Base;

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepositoryGuid<TEntity> : IRepository<TEntity, Guid> where TEntity : class, new()
{
}
