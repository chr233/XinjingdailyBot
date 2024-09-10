using SqlSugar;
using System.Reflection;

namespace XinjingdailyBot.Model.Services;
public class ChatIdSplitService : ISplitTableService
{
    /// <summary>
    /// 返回数据库中所有分表
    /// </summary>
    /// <param name="db"></param>
    /// <param name="EntityInfo"></param>
    /// <param name="tableInfos"></param>
    /// <returns></returns>
    public List<SplitTableInfo> GetAllTables(ISqlSugarClient db, EntityInfo EntityInfo, List<DbTableInfo> tableInfos)
    {
        List<SplitTableInfo> result = [];
        foreach (var item in tableInfos)
        {
            if (item.Name.Contains("_First")) //区分标识如果不用正则符复杂一些，防止找错表
            {
                var data = new SplitTableInfo {
                    TableName = item.Name //要用item.name不要写错了
                };
                result.Add(data);
            }
        }
        return result.OrderBy(it => it.TableName).ToList();//打断点看一下有没有查出所有分表
    }

    /// <summary>
    /// 获取分表字段的值
    /// </summary>
    /// <param name="db"></param>
    /// <param name="entityInfo"></param>
    /// <param name="splitType"></param>
    /// <param name="entityValue"></param>
    /// <returns></returns>
    public object GetFieldValue(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object entityValue)
    {
        var splitColumn = entityInfo.Columns.FirstOrDefault(it => it.PropertyInfo.GetCustomAttribute<SplitFieldAttribute>() != null);
        var value = splitColumn.PropertyInfo.GetValue(entityValue, null);
        return value;
    }
    /// <summary>
    /// 默认表名
    /// </summary>
    /// <param name="db"></param>
    /// <param name="entityInfo"></param>
    /// <returns></returns>
    public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo)
    {
        return string.Format("{0}_default", entityInfo.DbTableName);//目前模式少不需要分类(自带的有 日、周、月、季、年等进行区分)
    }

    public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo, SplitType type)
    {
        return string.Format("{0}_default", entityInfo.DbTableName);//目前模式少不需要分类(自带的有 日、周、月、季、年等进行区分)
    }

    public string GetTableName(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object fieldValue)
    {
        return string.Format("{0}_chat_{1}", entityInfo.DbTableName, fieldValue ?? "default"); //根据值按首字母
    }
}
