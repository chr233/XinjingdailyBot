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
        private readonly Dictionary<int, string[]> _tagKeywords = new();

        /// <summary>
        /// 初始化缓存
        /// </summary>
        /// <returns></returns>
        public async Task InitPostTagCache()
        {
            var tagCount = await CountAsync(x => x.Id > 0 && x.Id < 32);
            if (tagCount == 0)
            {
                _logger.LogInformation("未设置标签，正在创建内置标签");
                await InsertBuildInTags();
            }

            var tags = await GetListAsync(x => x.Id > 0 && x.Id < 32);
            if (tags?.Count > 0)
            {
                _tagCache.Clear();
                foreach (var tag in tags)
                {
                    if (string.IsNullOrEmpty(tag.Name))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(tag.OnText))
                    {
                        tag.OnText = "#" + tag.Name;
                    }
                    if (string.IsNullOrEmpty(tag.OffText))
                    {
                        tag.OffText = "#" + tag.Name.First() + new string('_', tag.Name.Length - 1);
                    }
                    if (string.IsNullOrEmpty(tag.HashTag))
                    {
                        tag.HashTag = "#" + tag.Name;
                    }

                    tag.Seg = 1 << tag.Id;
                    _tagCache.Add(tag.Id, tag);

                    if (!string.IsNullOrEmpty(tag.KeyWords))
                    {
                        var keyWords = tag.KeyWords.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        if (keyWords?.Length > 0)
                        {
                            _tagKeywords.Add(tag.Seg, keyWords);
                        }
                    }
                }

                await Storageable(tags).ExecuteCommandAsync();

                _logger.LogInformation("已加载 {Count} 个标签", tags.Count);
            }
            else
            {
                _logger.LogError("意外错误, 标签数据为空");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 创建内置标签
        /// </summary>
        /// <returns></returns>
        private async Task InsertBuildInTags()
        {
            List<Tags> tags = new()
            {
                new() { Id = 1,Name = "NSFW", KeyWords = "NSFW", AutoSpoiler = true },
                new() { Id = 2,Name = "我有一个朋友", KeyWords = "朋友|英雄", AutoSpoiler = false },
                new() { Id = 3,Name = "晚安", KeyWords = "晚安", AutoSpoiler = false },
                new() { Id = 4,Name = "AI怪图", KeyWords = "#AI", AutoSpoiler = false },
            };

            await Storageable(tags).ExecuteCommandAsync();
        }

        public Tags? GetTagById(int levelId)
        {
            if (_tagCache.TryGetValue(levelId, out var tag))
            {
                return tag;
            }
            return null;
        }

        /// <summary>
        /// 根据文本关键词设置稿件标签
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int FetchTags(string? text)
        {
            if (string.IsNullOrEmpty(text) || _tagCache.Count == 0)
            {
                return 0;
            }
            int tags = 0;
            foreach (var (seg, words) in _tagKeywords)
            {
                foreach (var word in words)
                {
                    if (text.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //TODO
                        tags += seg;
                        break;
                    }
                }
            }
            return tags;
        }

        public string GetTagsName(int tagNum)
        {
            var tagNames = GetTags(tagNum).Select(x=>x.Name);
            return string.Join(' ', tagNames);
        }

        public IEnumerable<Tags> GetTags(int tagNum)
        {
            List<Tags> tags = new();
            foreach (var (seg, tag) in _tagCache)
            {
                if ((seg & tagNum) > 0)
                {
                    tags.Add(tag);
                }
            }

            return tags;
        }
    }
}
