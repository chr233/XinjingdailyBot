using System.Linq.Expressions;
using XinjingDaily.Bot.Entry.Model;

namespace XinjingDaily.Bot.IRepository.Base;

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class, new()
{
    #region Create
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<int> InsertAsync(TEntity entity);
    /// <summary>
    /// 批量添加
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<int> InsertAsync(List<TEntity> entities);
    /// <summary>
    /// 批量添加
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<int> InsertAsync(TEntity[] entities);
    #endregion

    #region Update
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
    Task<bool> UpdateAsync(List<TEntity> entities);
    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(TEntity[] entities);
    #endregion

    #region Delete
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
    Task<bool> DeleteAsync(List<TEntity> entities);
    #endregion

    #region Update
    /// <summary>
    /// 根据ID查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TEntity?> QueryByIdAsync(TKey id);

    /// <summary>
    /// 查询所有
    /// </summary>
    /// <returns></returns>
    Task<List<TEntity>> QueryAllAsync();

    /// <summary>
    /// 根据条件查询
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<List<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PageData<TEntity>> QueryPageAsync(int pageIndex, int pageSize);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<PageData<TEntity>> QueryPageAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> predicate);

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
    Task<int> CountAsync();

    /// <summary>
    /// 统计数量
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    #endregion
}
