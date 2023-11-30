using SqlSugar;
using System.Data;
using System.Linq.Expressions;
using XinjingdailyBot.Model.Base;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace XinjingdailyBot.Repository.Base;


/// <summary>
/// 仓储层基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseRepository<T> : SimpleClient<T>, IBaseRepository<T> where T : BaseModel, new()
{
    protected BaseRepository(ISqlSugarClient context) : base(context)
    {
        Context = context;
    }

    #region add

    /// <summary>
    /// 插入实体
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public int Add(T t)
    {
        return Context.CopyNew().Insertable(t).IgnoreColumns(true).ExecuteCommand();
    }

    public int Insert(List<T> t)
    {
        return Context.CopyNew().Insertable(t).ExecuteCommand();
    }
    public int Insert(T parm, Expression<Func<T, object>>? iClumns = null, bool ignoreNull = true)
    {
        return Context.CopyNew().Insertable(parm).InsertColumns(iClumns).IgnoreColumns(ignoreNullColumn: ignoreNull).ExecuteCommand();
    }
    public IInsertable<T> Insertable(T t)
    {
        return Context.CopyNew().Insertable<T>(t);
    }
    #endregion add

    #region update
    public IUpdateable<T> Updateable(T entity)
    {
        return Context.CopyNew().Updateable(entity);
    }
    public int Update(T entity, bool ignoreNullColumns = false)
    {
        return Context.CopyNew().Updateable(entity).IgnoreColumns(ignoreNullColumns).ExecuteCommand();
    }

    public int Update(T entity, Expression<Func<T, object>> expression, bool ignoreAllNull = false)
    {
        return Context.CopyNew().Updateable(entity).UpdateColumns(expression).IgnoreColumns(ignoreAllNull).ExecuteCommand();
    }

    /// <summary>
    /// 根据实体类更新指定列 eg：Update(dept, it => new { it.Status }, f => depts.Contains(f.DeptId));只更新Status列，条件是包含
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="expression"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public int Update(T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where)
    {
        return Context.CopyNew().Updateable(entity).UpdateColumns(expression).Where(where).ExecuteCommand();
    }

    public int Update(SqlSugarClient client, T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where)
    {
        return client.CopyNew().Updateable(entity).UpdateColumns(expression).Where(where).ExecuteCommand();
    }

    /// <summary>
    /// 更新指定列 eg：Update(w => w.NoticeId == model.NoticeId, it => new SysNotice(){ Update_time = DateTime.Now, Title = "通知标题" });
    /// </summary>
    /// <param name="where"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    public int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns)
    {
        return Context.CopyNew().Updateable<T>().SetColumns(columns).Where(where).RemoveDataCache().ExecuteCommand();
    }
    #endregion update

    public DbResult<bool> UseTran(Action action)
    {
        try
        {
            var result = Context.CopyNew().Ado.UseTran(() => action());
            return result;
        }
        catch (Exception ex)
        {
            Context.CopyNew().Ado.RollbackTran();
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    public IStorageable<T> Storageable(T t)
    {
        return Context.CopyNew().Storageable<T>(t);
    }
    public IStorageable<T> Storageable(List<T> t)
    {
        return Context.CopyNew().Storageable(t);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="action">增删改查方法</param>
    /// <returns></returns>
    public DbResult<bool> UseTran(SqlSugarClient client, Action action)
    {
        try
        {
            var result = client.CopyNew().AsTenant().UseTran(() => action());
            return result;
        }
        catch (Exception ex)
        {
            client.CopyNew().AsTenant().RollbackTran();
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public bool UseTran2(Action action)
    {
        var result = Context.CopyNew().Ado.UseTran(() => action());
        return result.IsSuccess;
    }

    #region delete
    public IDeleteable<T> Deleteable()
    {
        return Context.CopyNew().Deleteable<T>();
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int Delete(object[] obj)
    {
        return Context.CopyNew().Deleteable<T>().In(obj).ExecuteCommand();
    }
    public int Delete(object id)
    {
        return Context.CopyNew().Deleteable<T>(id).ExecuteCommand();
    }
    public int DeleteTable()
    {
        return Context.CopyNew().Deleteable<T>().ExecuteCommand();
    }
    public bool Truncate()
    {
        return Context.CopyNew().DbMaintenance.TruncateTable<T>();
    }
    #endregion delete

    #region query

    public bool Any(Expression<Func<T, bool>> expression)
    {
        return Context.CopyNew().Queryable<T>().Where(expression).Any();
    }

    public ISugarQueryable<T> Queryable()
    {
        return Context.CopyNew().Queryable<T>();
    }
    
    /// <summary>
    /// 根据主值查询单条数据
    /// </summary>
    /// <param name="pkValue">主键值</param>
    /// <returns>泛型实体</returns>
    public T GetId(object pkValue)
    {
        return Context.CopyNew().Queryable<T>().InSingle(pkValue);
    }

    #endregion query

    /// <summary>
    /// 此方法不带output返回值
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public DataTable UseStoredProcedureToDataTable(string procedureName, List<SugarParameter> parameters)
    {
        return Context.CopyNew().Ado.UseStoredProcedure().GetDataTable(procedureName, parameters);
    }

    /// <summary>
    /// 带output返回值
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public (DataTable, List<SugarParameter>) UseStoredProcedureToTuple(string procedureName, List<SugarParameter> parameters)
    {
        var result = (Context.CopyNew().Ado.UseStoredProcedure().GetDataTable(procedureName, parameters), parameters);
        return result;
    }
}

