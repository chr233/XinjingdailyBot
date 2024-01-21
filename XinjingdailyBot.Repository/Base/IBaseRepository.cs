using SqlSugar;
using System.Data;
using System.Linq.Expressions;
using XinjingdailyBot.Model.Base;


namespace XinjingdailyBot.Repository.Base;


/// <summary>
/// 仓储处基类接口
/// </summary>
/// <typeparam name="T"></typeparam>
[Obsolete("重构")]
public interface IBaseRepository<T> : ISimpleClient<T> where T : BaseModel, new()
{
    #region add
    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Add(T t);

    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Insert(List<T> t);
    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="parm"></param>
    /// <param name="iClumns"></param>
    /// <param name="ignoreNull"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Insert(T parm, Expression<Func<T, object>>? iClumns = null, bool ignoreNull = true);

    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    IInsertable<T> Insertable(T t);
    #endregion add

    #region update
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    IUpdateable<T> Updateable(T entity);
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="ignoreNullColumns"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="expression"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Update(T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="client"></param>
    /// <param name="entity"></param>
    /// <param name="expression"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Update(SqlSugarClient client, T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="where"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns);

    #endregion update
    /// <summary>
    /// 存储
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    IStorageable<T> Storageable(T t);
    /// <summary>
    /// 存储
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    IStorageable<T> Storageable(List<T> t);

    #region delete
    /// <summary>
    /// 删除
    /// </summary>
    /// <returns></returns>
    [Obsolete("重构")]
    IDeleteable<T> Deleteable();
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Delete(object[] obj);
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    int Delete(object id);
    /// <summary>
    /// 删除
    /// </summary>
    /// <returns></returns>
    [Obsolete("重构")]
    int DeleteTable();
    /// <summary>
    /// 截断
    /// </summary>
    /// <returns></returns>
    [Obsolete("重构")]
    bool Truncate();

    #endregion delete

    #region query
    /// <summary>
    /// 任意
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    bool Any(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    [Obsolete("重构")]
    ISugarQueryable<T> Queryable();

    /// <summary>
    /// 获取主键
    /// </summary>
    /// <param name="pkValue"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    T GetId(object pkValue);

    #endregion query

    #region Procedure

    /// <summary>
    /// 存储过程
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    DataTable UseStoredProcedureToDataTable(string procedureName, List<SugarParameter> parameters);

    /// <summary>
    /// 存储过程
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    [Obsolete("重构")]
    (DataTable, List<SugarParameter>) UseStoredProcedureToTuple(string procedureName, List<SugarParameter> parameters);

    #endregion Procedure
}
