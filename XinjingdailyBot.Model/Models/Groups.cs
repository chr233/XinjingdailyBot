using SqlSugar;
using XinjingdailyBot.Model.Enums;
using XinjingdailyBot.Model.Enums.Base;

namespace XinjingdailyBot.Model.Models
{
    /// <summary>
    /// 用户权限组信息
    /// </summary>
    [SugarTable("group", TableDescription = "用户组")]
    public sealed record Groups : BaseModel
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        /// <summary>
        /// 权限名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 默认权限
        /// </summary>
        public UserRights DefaultRight { get; set; } = UserRights.None;
    }
}
