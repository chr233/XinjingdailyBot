using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    /// <summary>
    /// 用户等级仓储类
    /// </summary>
    [AppService(LifeTime.Singleton)]
    public class LevelRepository : BaseRepository<Levels>
    {
        private readonly ILogger<LevelRepository> _logger;

        /// <summary>
        /// 用户等级仓储类
        /// </summary>
        /// <param name="logger"></param>
        public LevelRepository(ILogger<LevelRepository> logger)
        {
            _logger = logger;
        }

        private Dictionary<int, Levels> _levelCache { get; } = new();

        /// <summary>
        /// 初始化缓存
        /// </summary>
        /// <returns></returns>
        public async Task InitLevelCache()
        {
            var defaultLevel = await GetFirstAsync(x => x.Id == 1);
            if (defaultLevel == null)
            {
                _logger.LogInformation("缺少默认等级，正在创建内置等级");
                await InsertBuildInLevels();
            }

            var levels = await GetListAsync();
            if (levels.Any())
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
        private async Task InsertBuildInLevels()
        {
            //请不要修改ID为0和1的字段
            var levels = new List<Levels>
            {              
                new Levels { Id = 0, Name = "Lv-" },
                new Levels { Id = 1, Name = "Lv0", MinExp = 0, MaxExp = 10 },
                new Levels { Id = 2, Name = "Lv1", MinExp = 11, MaxExp = 100 },
                new Levels { Id = 3, Name = "Lv2", MinExp = 101, MaxExp = 500 },
                new Levels { Id = 4, Name = "Lv3", MinExp = 501, MaxExp = 1000 },
                new Levels { Id = 5, Name = "Lv4", MinExp = 1001, MaxExp = 2000 },
                new Levels { Id = 6, Name = "Lv5", MinExp = 2001, MaxExp = 5000 },
                new Levels { Id = 7, Name = "Lv6", MinExp = 5001, MaxExp = 10000 },
                new Levels { Id = 8, Name = "Lv6+", MinExp = 10001 },
            };

            await Storageable(levels).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据ID获取等级
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public Levels? GetLevelById(int levelId)
        {
            if (_levelCache.TryGetValue(levelId, out var level))
            {
                return level;
            }
            return null;
        }

        /// <summary>
        /// 获取最大的等级ID
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevelId()
        {
            if (_levelCache.Count > 0)
            {
                return _levelCache.Keys.Max();
            }
            return 0;
        }

        /// <summary>
        /// 是否有ID对应等级
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool HasLevelId(int groupId)
        {
            var group = GetLevelById(groupId);
            return group != null;
        }

        /// <summary>
        /// 获取默认等级
        /// </summary>
        /// <returns></returns>
        public Levels? GetDefaultLevel()
        {
            return GetLevelById(1);
        }

        /// <summary>
        /// 获取等级名称
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public string GetLevelName(int levelId)
        {
            var level = GetLevelById(levelId);
            return level?.Name ?? "Lv-";
        }
    }
}
