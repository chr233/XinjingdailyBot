using SqlSugar;
using System.Linq.Expressions;
using XinjingDaily.Bot.Entry.Model;
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
    #region Create
    /// <inheritdoc />
    public virtual async Task<bool> InsertAsync(TEntity entity)
    {
        return await db
            .Insertable(entity)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> InsertAsync(List<TEntity> entities)
    {
        return await db
            .Insertable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> InsertAsync(TEntity[] entities)
    {
        return await db
            .Insertable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }
    #endregion

    #region Update
    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        return await db
            .Updateable(entity)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(List<TEntity> entities)
    {
        return await db
            .Updateable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(TEntity[] entities)
    {
        return await db
            .Updateable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }
    #endregion

    #region Delete
    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        return await db
            .Deleteable(entity)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(List<TEntity> entities)
    {
        return await db
            .Deleteable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }
    #endregion

    #region Query
    /// <inheritdoc />
    public virtual async Task<TEntity?> QueryByIdAsync(TKey id)
    {
        return await db
            .Queryable<TEntity>()
            .InSingleAsync(id)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db
            .Queryable<TEntity>()
            .Where(predicate)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> QueryAllAsync()
    {
        return await db
            .Queryable<TEntity>()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public virtual async Task<PageData<TEntity>> QueryPageAsync(int pageIndex, int pageSize)
    {
        var totalNumber = new RefAsync<int>(0);
        var totalPage = new RefAsync<int>(0);
        var result = await db
            .Queryable<TEntity>()
            .ToPageListAsync(pageIndex, pageSize, totalNumber, totalPage)
            .ConfigureAwait(false);

        return new PageData<TEntity>(pageIndex, result.Count, totalPage.Value, totalNumber.Value, result);
    }

    /// <inheritdoc />
    public virtual async Task<PageData<TEntity>> QueryPageAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> predicate)
    {
        var totalNumber = new RefAsync<int>(0);
        var totalPage = new RefAsync<int>(0);
        var result = await db
            .Queryable<TEntity>()
            .Where(predicate)
            .ToPageListAsync(pageIndex, pageSize, totalNumber, totalPage)
            .ConfigureAwait(false);

        return new PageData<TEntity>(pageIndex, result.Count, totalPage.Value, totalNumber.Value, result);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db
            .Queryable<TEntity>()
            .Where(predicate)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync()
    {
        return await db
            .Queryable<TEntity>()
            .CountAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await db
            .Queryable<TEntity>()
            .Where(predicate)
            .CountAsync()
            .ConfigureAwait(false);
    }
    #endregion
}
