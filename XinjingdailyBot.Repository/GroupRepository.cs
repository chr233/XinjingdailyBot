using Microsoft.Extensions.Logging;
using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    [AppService(ServiceLifetime = LifeTime.Singleton)]
    public class GroupRepository : BaseRepository<Groups>
    {
        private readonly ILogger<GroupRepository> _logger;
        private readonly Dictionary<int, Groups> _allGroups;

        public GroupRepository(
            ILogger<GroupRepository> logger,
            ISqlSugarClient? context = null
        ) : base(context)
        {
            _logger = logger;
            _allGroups = new();
        }

        public async Task<Groups?> GetGroupById(int groupId)
        {
            if (_allGroups.TryGetValue(groupId, out var group))
            {
                return group;
            }
            else
            {
                await FetchAllGroups();
                if (_allGroups.TryGetValue(groupId, out group))
                {
                    return group;
                }
            }
            return null;
        }
        public async Task<int> GetMaxGroupId()
        {
            if (!_allGroups.Any())
            {
                await FetchAllGroups();
            }
            return _allGroups.Keys.Max();
        }

        public async Task<bool> HasGroupId(int groupId)
        {
            var group = await GetGroupById(groupId);
            return group != null;
        }

        public Task<Groups?> GetDefaultGroup()
        {
            return GetGroupById(1);
        }

        private async Task FetchAllGroups()
        {
            var groups = await GetListAsync();
            _allGroups.Clear();
            foreach (var group in groups)
            {
                _allGroups.Add(group.Id, group);
            }

            _logger.LogInformation("已加载 {Count} 个群组", groups.Count);
        }
    }
}
