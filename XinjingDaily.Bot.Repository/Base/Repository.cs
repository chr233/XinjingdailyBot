using SqlSugar;
using System.Linq.Expressions;
using XinjingDaily.Bot.IRepository.Base;

namespace XinjingDaily.Bot.Repository.Base;

/// <summary>
/// 仓储基类
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public abstract class Repository<TEntity, TKey>(
    ISqlSugarClient db) : IRepository<TEntity, TKey> where TEntity : class, new()
{
    /// <inheritdoc />
    public virtual async Task<bool> AddAsync(TEntity entity)
    {
        return await db.Insertable(entity).ExecuteCommandAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> AddRangeAsync(List<TEntity> entities)
    {
        return await db.Insertable(entities).ExecuteCommandAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        return await db.Updateable(entity).ExecuteCommandAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateRangeAsync(List<TEntity> entities)
    {
        return await db.Updateable(entities).ExecuteCommandAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        return await db.Deleteable(entity).ExecuteCommandAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteRangeAsync(List<TEntity> entities)
    {
        return await db.Deleteable(entities).ExecuteCommandAsync() > 0;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await db.Queryable<TEntity>().InSingleAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await db.Queryable<TEntity>().ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db.Queryable<TEntity>().Where(predicate).ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<(List<TEntity>, int)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = db.Queryable<TEntity>();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var total = await query.CountAsync();
        var list = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

        return (list, total);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db.Queryable<TEntity>().Where(predicate).AnyAsync();
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = db.Queryable<TEntity>();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }
}
