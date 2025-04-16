using Microsoft.Extensions.Logging;
using SqlSugar;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Legacy;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.Repositorys;

/// <summary>
/// 用户等级仓储类
/// </summary>
/// <remarks>
/// 用户等级仓储类
/// </remarks>
/// <param name="_logger"></param>
/// <param name="_context"></param>
[AppService(LifeTime.Singleton)]
public class LevelRepository(
    ILogger<LevelRepository> _logger,
    ISqlSugarClient _context) : BaseRepository<LevelsLegacy>(_context)
{
    private Dictionary<int, LevelsLegacy> LevelCache { get; } = [];

    /// <summary>
    /// 初始化缓存
    /// </summary>
    /// <returns></returns>
    public async Task InitLevelCache()
    {
        var defaultLevel = await Queryable().FirstAsync(x => x.Id == 1).ConfigureAwait(false);
        if (defaultLevel == null)
        {
            _logger.LogInformation("缺少默认等级，正在创建内置等级");
            await InsertBuildInLevels().ConfigureAwait(false);
        }

        var levels = await Queryable().ToListAsync().ConfigureAwait(false);
        if (levels.Count != 0)
        {
            LevelCache.Clear();
            foreach (var level in levels)
            {
                LevelCache.Add(level.Id, level);
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
        var levels = new List<LevelsLegacy>
        {
            new LevelsLegacy { Id = 0, Name = "Lv-" },
            new LevelsLegacy { Id = 1, Name = "Lv0", MinExp = 0, MaxExp = 10 },
            new LevelsLegacy { Id = 2, Name = "Lv1", MinExp = 11, MaxExp = 100 },
            new LevelsLegacy { Id = 3, Name = "Lv2", MinExp = 101, MaxExp = 500 },
            new LevelsLegacy { Id = 4, Name = "Lv3", MinExp = 501, MaxExp = 1000 },
            new LevelsLegacy { Id = 5, Name = "Lv4", MinExp = 1001, MaxExp = 2000 },
            new LevelsLegacy { Id = 6, Name = "Lv5", MinExp = 2001, MaxExp = 5000 },
            new LevelsLegacy { Id = 7, Name = "Lv6", MinExp = 5001, MaxExp = 10000 },
            new LevelsLegacy { Id = 8, Name = "Lv6+", MinExp = 10001 },
        };

        await Storageable(levels).ExecuteCommandAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 根据ID获取等级
    /// </summary>
    /// <param name="levelId"></param>
    /// <returns></returns>
    public LevelsLegacy? GetLevelById(int levelId)
    {
        if (LevelCache.TryGetValue(levelId, out var level))
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
        if (LevelCache.Count > 0)
        {
            return LevelCache.Keys.Max();
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
    public LevelsLegacy? GetDefaultLevel()
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

    /// <summary>
    /// 根据经验获取等级组
    /// </summary>
    /// <param name="totalExp"></param>
    /// <returns></returns>
    public LevelsLegacy? GetLevelByExp(ulong totalExp)
    {
        foreach (var (_, level) in LevelCache)
        {
            var min = level.MinExp;
            var max = level.MaxExp;
            if (min == 0 && max == 0)
            {
                continue;
            }

            if ((min <= totalExp || min == 0) && (max >= totalExp || max == 0))
            {
                return level;
            }
        }
        return GetDefaultLevel();
    }
}
