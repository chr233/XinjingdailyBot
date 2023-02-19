using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    [AppService(LifeTime.Singleton)]
    public class TagRepository : BaseRepository<Tags>
    {
        private readonly ILogger<GroupRepository> _logger;

        public TagRepository(ILogger<GroupRepository> logger)
        {
            _logger = logger;
        }

        private readonly Dictionary<int, Tags> _tagCache = new();

        /// <summary>
        /// 初始化缓存
        /// </summary>
        /// <returns></returns>
        public async Task InitPostTagCache()
        {
            var defaultLevel = await GetFirstAsync(x => x.Id == 1);
            if (defaultLevel == null)
            {
                _logger.LogInformation("缺少默认等级，正在创建内置等级");
                await InsertBuildInTags();
            }

            var levels = await GetListAsync();
            if (levels?.Count > 0)
            {
                _levelCache.Clear();
                foreach (var level in levels)
                {
                    _levelCache.Add(level.Id, level);
                }
                _logger.LogInformation("已加载 {Count} 个等级", levels.Count);
            }
            else
            {
                _logger.LogError("意外错误, 等级数据为空");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 创建内置群组
        /// </summary>
        /// <returns></returns>
        private async Task InsertBuildInTags()
        {
            //请不要修改ID为0和1的字段
            List<Tags> tags = new()
            {
                new() {
                    Id = 1,
                    TagSeg = 1,
                    OnText = "#NSFW",
                    OffText = "#N____",
                    HashTag = "#NSFW",
                    KeyWords = "NSFW",
                    AutoSpoiler = true,
                },
                new() {
                    Id = 2,
                    TagSeg = 2,
                    OnText = "#我有一个碰",
                    OffText = "#N____",
                    HashTag = "#NSFW",
                    KeyWords = "NSFW",
                    AutoSpoiler = true,
                },
                new() {
                    Id = 3,
                    TagSeg = 4,
                    OnText = "#NSFW",
                    OffText = "#N____",
                    HashTag = "#NSFW",
                    KeyWords = "NSFW",
                    AutoSpoiler = true,
                },
                new() {
                    Id = 0,
                    TagSeg = 1,
                    OnText = "#NSFW",
                    OffText = "#N____",
                    HashTag = "#NSFW", 
                    KeyWords = "NSFW",
                    AutoSpoiler = true,
                },
               
            };

            await Storageable(tags).ExecuteCommandAsync();
        }

        //public Levels? GetLevelById(int levelId)
        //{
        //    if (_levelCache.TryGetValue(levelId, out var level))
        //    {
        //        return level;
        //    }
        //    return null;
        //}
        //public int GetMaxGroupId()
        //{
        //    if (_levelCache.Count > 0)
        //    {
        //        return _levelCache.Keys.Max();
        //    }
        //    return 0;
        //}

        //public bool HasGroupId(int groupId)
        //{
        //    var group = GetLevelById(groupId);
        //    return group != null;
        //}

        //public Levels? GetDefaultLevel()
        //{
        //    return GetLevelById(1);
        //}

        //public string GetLevelName(int levelId)
        //{
        //    var level = GetLevelById(levelId);
        //    return level?.Name ?? "Lv-";
        //}
    }
}
