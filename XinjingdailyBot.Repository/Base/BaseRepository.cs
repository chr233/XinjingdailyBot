using SqlSugar;
using System.Linq.Expressions;
using XinjingdailyBot.Model.Base;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace XinjingdailyBot.Repository.Base;

/// <summary>
/// 仓储层基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseRepository<T>(ISqlSugarClient _context) where T : BaseModel, new()
{
    protected readonly ISqlSugarClient _context = _context;

    #region add

    /// <summary>
    /// 插入实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected int Add(T t)
    {
        return _context.Insertable(t).IgnoreColumns(true).ExecuteCommand();
    }

    protected int Insert(List<T> t)
    {
        return _context.Insertable(t).ExecuteCommand();
    }

    protected Task<int> InsertAsync(List<T> t)
    {
        return _context.Insertable(t).ExecuteCommandAsync();
    }

    protected int Insert(T parm, Expression<Func<T, object>>? iClumns = null, bool ignoreNull = true)
    {
        return _context.Insertable(parm).InsertColumns(iClumns).IgnoreColumns(ignoreNullColumn: ignoreNull).ExecuteCommand();
    }
    protected IInsertable<T> Insertable(T t)
    {
        return _context.Insertable(t);
    }
    #endregion add

    #region update
    protected IUpdateable<T> Updateable(T entity)
    {
        return _context.Updateable(entity);
    }
    protected IUpdateable<T> Updateable(List<T> entities)
    {
        return _context.Updateable(entities);
    }

    protected int Update(T entity, bool ignoreNullColumns = false)
    {
        return _context.Updateable(entity).IgnoreColumns(ignoreNullColumns).ExecuteCommand();
    }

    protected int Update(T entity, Expression<Func<T, object>> expression, bool ignoreAllNull = false)
    {
        return _context.Updateable(entity).UpdateColumns(expression).IgnoreColumns(ignoreAllNull).ExecuteCommand();
    }

    /// <summary>
    /// 根据实体类更新指定列 eg：Update(dept, it => new { it.Status }, f => depts.Contains(f.DeptId));只更新Status列，条件是包含
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="expression"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    protected int Update(T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where)
    {
        return _context.Updateable(entity).UpdateColumns(expression).Where(where).ExecuteCommand();
    }

    protected int Update(SqlSugarClient client, T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where)
    {
        return client.Updateable(entity).UpdateColumns(expression).Where(where).ExecuteCommand();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="list"></param>
    /// <param name="isNull">默认为true</param>
    /// <returns></returns>
    protected int Update(T entity, List<string>? list = null, bool isNull = true)
    {
        list ??= ["Create_By", "Create_time"];

        return _context.Updateable(entity).IgnoreColumns(isNull).IgnoreColumns([.. list]).ExecuteCommand();
    }

    /// <summary>
    /// 更新指定列 eg：Update(w => w.NoticeId == model.NoticeId, it => new SysNotice(){ Update_time = DateTime.Now, Title = "通知标题" });
    /// </summary>
    /// <param name="where"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    protected int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns)
    {
        return _context.Updateable<T>().SetColumns(columns).Where(where).RemoveDataCache().ExecuteCommand();
    }
    #endregion update

    protected IStorageable<T> Storageable(T t)
    {
        return _context.Storageable(t);
    }
    protected IStorageable<T> Storageable(List<T> t)
    {
        return _context.Storageable(t);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="action">增删改查方法</param>
    /// <returns></returns>
    protected DbResult<bool> UseTran(SqlSugarClient client, Action action)
    {
        try
        {
            var result = client.AsTenant().UseTran(() => action());
            return result;
        }
        catch (Exception ex)
        {
            client.AsTenant().RollbackTran();
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    protected bool UseTran2(Action action)
    {
        var result = _context.Ado.UseTran(() => action());
        return result.IsSuccess;
    }

    #region delete
    protected IDeleteable<T> Deleteable()
    {
        return _context.Deleteable<T>();
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected int Delete(object[] obj)
    {
        return _context.Deleteable<T>().In(obj).ExecuteCommand();
    }
    protected int Delete(object id)
    {
        return _context.Deleteable<T>(id).ExecuteCommand();
    }
    protected int DeleteTable()
    {
        return _context.Deleteable<T>().ExecuteCommand();
    }
    protected bool Truncate()
    {
        return _context.DbMaintenance.TruncateTable<T>();
    }
    #endregion delete

    #region query

    protected bool Any(Expression<Func<T, bool>> expression)
    {
        return _context.Queryable<T>().Where(expression).Any();
    }

    protected ISugarQueryable<T> Queryable()
    {
        return _context.Queryable<T>();
    }

    protected (List<T>, int) QueryableToPage(Expression<Func<T, bool>> expression, int pageIndex = 0, int pageSize = 10)
    {
        var totalNumber = 0;
        var list = _context.Queryable<T>().Where(expression).ToPageList(pageIndex, pageSize, ref totalNumber);
        return (list, totalNumber);
    }

    protected (List<T>, int) QueryableToPage(Expression<Func<T, bool>> expression, string order, int pageIndex = 0, int pageSize = 10)
    {
        var totalNumber = 0;
        var list = _context.Queryable<T>().Where(expression).OrderBy(order).ToPageList(pageIndex, pageSize, ref totalNumber);
        return (list, totalNumber);
    }

    protected (List<T>, int) QueryableToPage(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderFiled, string orderBy, int pageIndex = 0, int pageSize = 10)
    {
        var totalNumber = 0;

        if (orderBy.Equals("DESC", StringComparison.OrdinalIgnoreCase))
        {
            var list = _context.Queryable<T>().Where(expression).OrderBy(orderFiled, OrderByType.Desc).ToPageList(pageIndex, pageSize, ref totalNumber);
            return (list, totalNumber);
        }
        else
        {
            var list = _context.Queryable<T>().Where(expression).OrderBy(orderFiled, OrderByType.Asc).ToPageList(pageIndex, pageSize, ref totalNumber);
            return (list, totalNumber);
        }
    }

    protected List<T> SqlQueryToList(string sql, object? obj = null)
    {
        return _context.Ado.SqlQuery<T>(sql, obj);
    }

    /// <summary>
    /// 根据主值查询单条数据
    /// </summary>
    /// <param name="pkValue">主键值</param>
    /// <returns>泛型实体</returns>
    protected T GetId(object pkValue)
    {
        return _context.Queryable<T>().InSingle(pkValue);
    }

    /// <summary>
    /// 查询所有数据(无分页,请慎用)
    /// </summary>
    /// <returns></returns>
    protected List<T> GetAll(bool useCache = false, int cacheSecond = 3600)
    {
        return _context.Queryable<T>().WithCacheIF(useCache, cacheSecond).ToList();
    }

    #endregion query
}

