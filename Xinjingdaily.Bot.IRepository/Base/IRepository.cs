using System.Linq.Expressions;

namespace XinjingDaily.Bot.IRepository.Base;

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class, new()
{
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<bool> AddAsync(TEntity entity);

    /// <summary>
    /// 批量添加
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<bool> AddRangeAsync(List<TEntity> entities);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(TEntity entity);

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<bool> UpdateRangeAsync(List<TEntity> entities);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(TEntity entity);

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<bool> DeleteRangeAsync(List<TEntity> entities);

    /// <summary>
    /// 根据ID查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// 根据条件查询
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<List<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<(List<TEntity>, int)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>>? predicate = null);

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 统计数量
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
}
