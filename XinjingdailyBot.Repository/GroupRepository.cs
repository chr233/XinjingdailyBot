using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    [AppService(ServiceLifetime = LifeTime.Singleton)]
    public class GroupRepository : BaseRepository<Groups>
    {
        private readonly ILogger<GroupRepository> _logger;

        public GroupRepository(ILogger<GroupRepository> logger)
        {
            _logger = logger;
        }

        private readonly Dictionary<int, Groups> _groupCache = new();

        /// <summary>
        /// 初始化缓存
        /// </summary>
        /// <returns></returns>
        public async Task InitGroupCache()
        {
            var defaultGroup = await GetFirstAsync(x => x.Id == 1);
            if (defaultGroup == null)
            {
                _logger.LogInformation("缺少默认群组，正在创建内置群组");
                await InsertBuildInGroups();
            }

            var groups = await GetListAsync();
            if (groups?.Count > 0)
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
            List<Groups> groups = new()
            {
                new() { Id = 0, Name = "封禁用户", DefaultRight = UserRights.None },
                new() { Id = 1, Name = "普通用户", DefaultRight = UserRights.SendPost | UserRights.NormalCmd },
                new() { Id = 10, Name = "审核员", DefaultRight = UserRights.SendPost | UserRights.ReviewPost | UserRights.NormalCmd },
                new() { Id = 11, Name = "发布员", DefaultRight = UserRights.SendPost | UserRights.DirectPost | UserRights.NormalCmd },
                new() { Id = 20, Name = "狗管理", DefaultRight = UserRights.SendPost | UserRights.ReviewPost | UserRights.DirectPost | UserRights.NormalCmd | UserRights.AdminCmd },
                new() { Id = 30, Name = "超级狗管理", DefaultRight = UserRights.ALL },
                new() { Id = 50, Name = "*超级狗管理*", DefaultRight = UserRights.ALL },
            };

            await Storageable(groups).ExecuteCommandAsync();
        }

        public Groups? GetGroupById(int groupId)
        {
            if (_groupCache.TryGetValue(groupId, out var group))
            {
                return group;
            }
            return null;
        }
        public int GetMaxGroupId()
        {
            if (_groupCache.Count > 0)
            {
                return _groupCache.Keys.Max();
            }
            return 0;
        }

        public bool HasGroupId(int groupId)
        {
            var group = GetGroupById(groupId);
            return group != null;
        }

        public Groups? GetDefaultGroup()
        {
            return GetGroupById(1);
        }

        public string GetGroupName(int groupId)
        {
            var group = GetGroupById(groupId);
            return group?.Name ?? "未知群组";
        }
    }
}
