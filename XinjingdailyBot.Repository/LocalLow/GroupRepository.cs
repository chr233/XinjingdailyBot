using Microsoft.Extensions.Logging;
using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.Repositorys;

/// <summary>
/// 用户组仓储类
/// </summary>
[AppService(LifeTime.Singleton)]
public class GroupRepository(
    ILogger<GroupRepository> _logger,
    ISqlSugarClient _context) : BaseRepository<Groups>(_context)
{
    private readonly Dictionary<int, Groups> _groupCache = [];

    /// <summary>
    /// 初始化缓存
    /// </summary>
    /// <returns></returns>
    public async Task InitGroupCache()
    {
        var defaultGroup = await Queryable().FirstAsync(x => x.Id == 1).ConfigureAwait(false);
        if (defaultGroup == null)
        {
            _logger.LogInformation("缺少默认群组，正在创建内置群组");
            await InsertBuildInGroups().ConfigureAwait(false);
        }

        var groups = await Queryable().ToListAsync().ConfigureAwait(false);
        if (groups.Count != 0)
        {
            _groupCache.Clear();
            foreach (var group in groups)
            {
                _groupCache.Add(group.Id, group);
            }
            _logger.LogInformation("已加载 {Count} 个群组", groups.Count);
        }
        else
        {
            _logger.LogError("意外错误, 群组数据为空");
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// 创建内置群组
    /// </summary>
    /// <returns></returns>
    private async Task InsertBuildInGroups()
    {
        //请不要修改ID为0和1的字段
        var groups = new List<Groups>
        {
            new Groups { Id = 0, Name = "封禁用户", DefaultRight = EUserRights.None },
            new Groups { Id = 1, Name = "普通用户", DefaultRight = EUserRights.SendPost | EUserRights.NormalCmd },
            new Groups { Id = 10, Name = "审核员", DefaultRight = EUserRights.SendPost | EUserRights.ReviewPost | EUserRights.NormalCmd },
            new Groups { Id = 11, Name = "发布员", DefaultRight = EUserRights.SendPost | EUserRights.DirectPost | EUserRights.NormalCmd },
            new Groups { Id = 20, Name = "狗管理", DefaultRight = EUserRights.SendPost | EUserRights.ReviewPost | EUserRights.DirectPost | EUserRights.NormalCmd | EUserRights.AdminCmd },
            new Groups { Id = 30, Name = "超级狗管理", DefaultRight = EUserRights.ALL },
            new Groups { Id = 50, Name = "*超级狗管理*", DefaultRight = EUserRights.ALL },
        };

        await Storageable(groups).ExecuteCommandAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 获取群组
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public Groups? GetGroupById(int groupId)
    {
        if (_groupCache.TryGetValue(groupId, out var group))
        {
            return group;
        }
        return null;
    }

    /// <summary>
    /// 获取最大群组ID
    /// </summary>
    /// <returns></returns>
    public int GetMaxGroupId()
    {
        if (_groupCache.Count > 0)
        {
            return _groupCache.Keys.Max();
        }
        return 0;
    }

    /// <summary>
    /// 判断群组是否存在
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public bool HasGroupId(int groupId)
    {
        var group = GetGroupById(groupId);
        return group != null;
    }

    /// <summary>
    /// 获取默认群组
    /// </summary>
    /// <returns></returns>
    public Groups? GetDefaultGroup()
    {
        return GetGroupById(1);
    }

    /// <summary>
    /// 获取群组名称
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public string GetGroupName(int groupId)
    {
        var group = GetGroupById(groupId);
        return group?.Name ?? "未知群组";
    }
}
