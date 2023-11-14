using SqlSugar;
using System.Data;
using System.Linq.Expressions;
using XinjingdailyBot.Model.Base;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace XinjingdailyBot.Repository.Base;


/// <summary>
/// 仓储处基类接口
/// </summary>
/// <typeparam name="T"></typeparam>
[Obsolete("重构")]
public interface IBaseRepository<T> : ISimpleClient<T> where T : BaseModel, new()
{
    #region add
    [Obsolete("重构")]
    int Add(T t);

    [Obsolete("重构")]
    int Insert(List<T> t);
    [Obsolete("重构")]
    int Insert(T parm, Expression<Func<T, object>>? iClumns = null, bool ignoreNull = true);

    [Obsolete("重构")]
    IInsertable<T> Insertable(T t);
    #endregion add

    #region update
    [Obsolete("重构")]
    IUpdateable<T> Updateable(T entity);
    [Obsolete("重构")]
    int Update(T entity, bool ignoreNullColumns = false);

    /// <summary>
    /// 只更新表达式的值
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="expression"></param>
    /// <param name="ignoreAllNull"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Update(T entity, Expression<Func<T, object>> expression, bool ignoreAllNull = false);

    [Obsolete("重构")]
    int Update(T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where);

    [Obsolete("重构")]
    int Update(SqlSugarClient client, T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where);

    [Obsolete("重构")]
    int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns);

    #endregion update
    [Obsolete("重构")]
    IStorageable<T> Storageable(T t);
    [Obsolete("重构")]
    IStorageable<T> Storageable(List<T> t);
    [Obsolete("重构")]
    DbResult<bool> UseTran(Action action);

    [Obsolete("重构")]
    DbResult<bool> UseTran(SqlSugarClient client, Action action);

    [Obsolete("重构")]
    bool UseTran2(Action action);

    #region delete
    [Obsolete("重构")]
    IDeleteable<T> Deleteable();
    [Obsolete("重构")]
    int Delete(object[] obj);
    [Obsolete("重构")]
    int Delete(object id);
    [Obsolete("重构")]
    int DeleteTable();
    [Obsolete("重构")]
    bool Truncate();

    #endregion delete

    #region query
    /// <summary>
    /// 根据条件查询分页数据
    /// </summary>
    /// <param name="where"></param>
    /// <param name="parm"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    PagedInfo<T> GetPages(Expression<Func<T, bool>> where, PagerInfo parm);

    [Obsolete("重构")]
    PagedInfo<T> GetPages(Expression<Func<T, bool>> where, PagerInfo parm, Expression<Func<T, object>> order, OrderByType orderEnum = OrderByType.Asc);
    [Obsolete("重构")]
    PagedInfo<T> GetPages(Expression<Func<T, bool>> where, PagerInfo parm, Expression<Func<T, object>> order, string orderByType);

    [Obsolete("重构")]
    bool Any(Expression<Func<T, bool>> expression);

    [Obsolete("重构")]
    ISugarQueryable<T> Queryable();
    [Obsolete("重构")]
    List<T> GetAll(bool useCache = false, int cacheSecond = 3600);

    [Obsolete("重构")]
    (List<T>, int) QueryableToPage(Expression<Func<T, bool>> expression, int pageIndex = 0, int pageSize = 10);

    [Obsolete("重构")]
    (List<T>, int) QueryableToPage(Expression<Func<T, bool>> expression, string order, int pageIndex = 0, int pageSize = 10);

    [Obsolete("重构")]
    (List<T>, int) QueryableToPage(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderFiled, string orderBy, int pageIndex = 0, int pageSize = 10);

    [Obsolete("重构")]
    List<T> SqlQueryToList(string sql, object? obj);

    [Obsolete("重构")]
    T GetId(object pkValue);

    #endregion query

    #region Procedure

    [Obsolete("重构")]
    DataTable UseStoredProcedureToDataTable(string procedureName, List<SugarParameter> parameters);

    [Obsolete("重构")]
    (DataTable, List<SugarParameter>) UseStoredProcedureToTuple(string procedureName, List<SugarParameter> parameters);

    #endregion Procedure
}
