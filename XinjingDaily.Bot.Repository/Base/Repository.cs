using SqlSugar;
using System.Data;
using System.Dynamic;
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
    #region Base
    /// <inheritdoc/>
    protected IInsertable<TEntity> Insertable(List<TEntity> insertObjs)
    {
        return db.Insertable(insertObjs);
    }
    /// <inheritdoc/>
    protected IInsertable<TEntity> Insertable(TEntity insertObj)
    {
        return db.Insertable(insertObj);
    }
    /// <inheritdoc/>
    protected IInsertable<TEntity> Insertable(TEntity[] insertObjs)
    {
        return db.Insertable(insertObjs);
    }

    /// <inheritdoc/>
    protected IUpdateable<TEntity> Updateable()
    {
        return db.Updateable<TEntity>();
    }
    /// <inheritdoc/>
    protected IUpdateable<TEntity> Updateable(List<TEntity> updateObjs)
    {
        return db.Updateable(updateObjs);
    }
    /// <inheritdoc/>
    protected IUpdateable<TEntity> Updateable(TEntity updateObj)
    {
        return db.Updateable(updateObj);
    }
    /// <inheritdoc/>
    protected IUpdateable<TEntity> Updateable(TEntity[] updateObjs)
    {
        return db.Updateable(updateObjs);
    }

    /// <inheritdoc/>
    protected IDeleteable<TEntity> Deleteable()
    {
        return db.Deleteable<TEntity>();
    }

    /// <inheritdoc/>
    protected IDeleteable<TEntity> Deleteable(List<TEntity> deleteObjs)
    {
        return db.Deleteable(deleteObjs);
    }

    /// <inheritdoc/>
    protected IDeleteable<TEntity> Deleteable(Expression<Func<TEntity, bool>> expression)
    {
        return db.Deleteable(expression);
    }
    /// <inheritdoc/>
    protected IDeleteable<TEntity> Deleteable(TEntity deleteObj)
    {
        return db.Deleteable(deleteObj);
    }

    /// <inheritdoc/>
    protected ISugarQueryable<ExpandoObject> Queryable(string tableName, string shortName)
    {
        return db.Queryable(tableName, shortName);
    }
    /// <inheritdoc/>
    protected ISugarQueryable<TEntity> Queryable()
    {
        return db.Queryable<TEntity>();
    }
    /// <inheritdoc/>
    protected ISugarQueryable<TEntity> Queryable(ISugarQueryable<TEntity> queryable)
    {
        return db.Queryable(queryable);
    }
    /// <inheritdoc/>
    protected ISugarQueryable<TEntity> Queryable(ISugarQueryable<TEntity> queryable, string shortName)
    {
        return db.Queryable(queryable, shortName);
    }
    /// <inheritdoc/>
    protected ISugarQueryable<TEntity> Queryable(string shortName)
    {
        return db.Queryable<TEntity>(shortName);
    }

    /// <inheritdoc/>
    protected IStorageable<TEntity> Storageable(TEntity[] dataList)
    {
        return db.Storageable(dataList);
    }
    /// <inheritdoc/>
    protected IStorageable<TEntity> Storageable(IList<TEntity> dataList)
    {
        return db.Storageable(dataList);
    }
    /// <inheritdoc/>
    protected StorageableDataTable Storageable(List<Dictionary<string, object>> dictionaryList, string tableName)
    {
        return db.Storageable(dictionaryList, tableName);
    }
    /// <inheritdoc/>
    protected StorageableDataTable Storageable(Dictionary<string, object> dictionary, string tableName)
    {
        return db.Storageable(dictionary, tableName);
    }
    /// <inheritdoc/>
    protected IStorageable<TEntity> Storageable(List<TEntity> dataList)
    {
        return db.Storageable(dataList);
    }
    /// <inheritdoc/>
    protected IStorageable<TEntity> Storageable(TEntity data)
    {
        return db.Storageable(data);
    }
    /// <inheritdoc/>
    protected StorageableDataTable Storageable(DataTable data)
    {
        return db.Storageable(data);
    }
    #endregion

    #region Create
    /// <inheritdoc />
    public virtual async Task<int> InsertAsync(TEntity entity)
    {
        return await Insertable(entity)
            .ExecuteReturnIdentityAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> InsertAsync(List<TEntity> entities)
    {
        return await Insertable(entities)
            .ExecuteReturnIdentityAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> InsertAsync(TEntity[] entities)
    {
        return await Insertable(entities)
            .ExecuteReturnIdentityAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 批量插入或更新（存在则更新，不存在则插入）
    /// </summary>
    public virtual async Task<int> InsertOrUpdateAsync(List<TEntity> entities)
    {
        var storage = await Storageable(entities)
            .ToStorageAsync()
            .ConfigureAwait(false);

        // 执行插入
        await storage.AsInsertable.ExecuteCommandAsync().ConfigureAwait(false);
        // 执行更新
        await storage.AsUpdateable.ExecuteCommandAsync().ConfigureAwait(false);

        return entities.Count;
    }

    /// <summary>
    /// 批量插入（忽略已存在的记录）
    /// </summary>
    public virtual async Task<int> InsertOrIgnoreAsync(List<TEntity> entities)
    {
        var storage = await Storageable(entities)
            .SplitInsert(it => !it.Any()) // 数据库不存在则插入
            .SplitIgnore(it => it.Any())  // 数据库存在则忽略
            .ToStorageAsync()
            .ConfigureAwait(false);

        return await storage.AsInsertable.ExecuteCommandAsync().ConfigureAwait(false);
    }

    #endregion

    #region Update
    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        return await Updateable(entity)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(List<TEntity> entities)
    {
        return await Updateable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> UpdateAsync(TEntity[] entities)
    {
        return await Updateable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }
    #endregion

    #region Delete


    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        return await Deleteable(entity)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(List<TEntity> entities)
    {
        return await Deleteable(entities)
            .ExecuteCommandAsync()
            .ConfigureAwait(false) > 0;
    }
    #endregion

    #region Query
    /// <inheritdoc />
    public virtual async Task<TEntity?> QueryByIdAsync(TKey id)
    {
        return await Queryable()
            .InSingleAsync(id)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Queryable()
            .Where(predicate)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> QueryAllAsync()
    {
        return await Queryable()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<PageData<TEntity>> QueryPageAsync(int pageIndex, int pageSize)
    {
        var totalNumber = new RefAsync<int>(0);
        var totalPage = new RefAsync<int>(0);
        var result = await Queryable()
            .ToPageListAsync(pageIndex, pageSize, totalNumber, totalPage)
            .ConfigureAwait(false);

        return new PageData<TEntity>(pageIndex, result.Count, totalPage.Value, totalNumber.Value, result);
    }

    /// <inheritdoc />
    public virtual async Task<PageData<TEntity>> QueryPageAsync(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> predicate)
    {
        var totalNumber = new RefAsync<int>(0);
        var totalPage = new RefAsync<int>(0);
        var result = await Queryable()
            .Where(predicate)
            .ToPageListAsync(pageIndex, pageSize, totalNumber, totalPage)
            .ConfigureAwait(false);

        return new PageData<TEntity>(pageIndex, result.Count, totalPage.Value, totalNumber.Value, result);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Queryable()
            .Where(predicate)
            .AnyAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync()
    {
        return await Queryable()
            .CountAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Queryable()
            .Where(predicate)
            .CountAsync()
            .ConfigureAwait(false);
    }
    #endregion
}
