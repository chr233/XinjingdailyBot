using Microsoft.Extensions.Logging;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Localization;
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

        /// <summary>
        /// 标签缓存, Key=Id
        /// </summary>
        private readonly Dictionary<int, Tags> _tagCache = new();
        /// <summary>
        /// 标签缓存, Key=Id
        /// </summary>
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
                    if (string.IsNullOrEmpty(tag.Name) || tag.Id <= 0)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(tag.Payload))
                    {
                        tag.Payload = tag.Name;
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

                    tag.Payload = tag.Payload.ToLowerInvariant();
                    tag.Seg = 1 << tag.Id - 1;
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
                new() { Id = 1,Name = "NSFW", Payload = "nsfw", KeyWords = "NSFW", AutoSpoiler = true, WarnText = Langs.NSFWWarning },
                new() { Id = 2,Name = "我有一个朋友", Payload = "friend", KeyWords = "朋友|英雄", AutoSpoiler = false },
                new() { Id = 3,Name = "晚安", Payload = "wanan", KeyWords = "晚安", AutoSpoiler = false },
                new() { Id = 4,Name = "AI怪图", Payload = "ai", KeyWords = "#AI", AutoSpoiler = false },
            };

            await Storageable(tags).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据ID获取标签
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public Tags? GetTagById(int tagId)
        {
            if (_tagCache.TryGetValue(tagId, out var tag))
            {
                return tag;
            }
            return null;
        }

        /// <summary>
        /// 根据Payload获取标签
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public Tags? GetTagByPayload(string payload)
        {
            foreach (var tag in _tagCache.Values)
            {
                if (tag.Payload == payload)
                {
                    return tag;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tags> GetAllTags()
        {
            return _tagCache.Values;
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
            int tagNum = 0;
            foreach (var (seg, words) in _tagKeywords)
            {
                foreach (var word in words)
                {
                    if (text.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tagNum += seg;
                        break;
                    }
                }
            }
            return tagNum;
        }

        /// <summary>
        /// 获取激活的标签
        /// </summary>
        /// <param name="tagNum"></param>
        /// <returns></returns>
        public IEnumerable<Tags> GetActiviedTags(int tagNum)
        {
            List<Tags> tags = new();
            foreach (var tag in _tagCache.Values)
            {
                if ((tag.Seg & tagNum) > 0)
                {
                    tags.Add(tag);
                }
            }

            return tags;
        }

        /// <summary>
        /// 获取激活标签的 HashTag
        /// </summary>
        /// <param name="tagNum"></param>
        /// <returns></returns>
        public string GetActiviedHashTags(int tagNum)
        {
            var tagHashs = GetActiviedTags(tagNum).Select(x => x.HashTag);
            return string.Join(' ', tagHashs);
        }

        /// <summary>
        /// 获取激活标签的名称
        /// </summary>
        /// <param name="tagNum"></param>
        /// <returns></returns>
        public string GetActiviedTagsName(int tagNum)
        {
            var tagNames = GetActiviedTags(tagNum).Select(x => x.Name);
            return tagNames.Any() ? string.Join(' ', tagNames) : "无";
        }

        /// <summary>
        /// 获取激活标签的警告
        /// </summary>
        /// <param name="tagNum"></param>
        /// <returns></returns>
        public string? GetActivedTagWarnings(int tagNum)
        {
            var tagWarnings = GetActiviedTags(tagNum).Select(x => x.WarnText).Where(x => !string.IsNullOrEmpty(x));
            return tagWarnings.Any() ? string.Join("\r\n", tagWarnings) : null;
        }

        /// <summary>
        /// 获取标签的Payload
        /// </summary>
        /// <param name="tagNum"></param>
        /// <returns></returns>
        public IEnumerable<TagPayload> GetTagsPayload(int tagNum)
        {
            List<TagPayload> tags = new();
            foreach (var (seg, tag) in _tagCache)
            {
                bool status = (seg & tagNum) > 0;

                tags.Add(new TagPayload(status ? tag.OnText : tag.OffText, tag.Payload));
            }

            return tags;
        }
    }

    /// <summary>
    /// Tag结构体
    /// </summary>
    public sealed record TagPayload
    {
        public string DisplayName { get; set; } = "";
        public string Payload { get; set; } = "";

        public TagPayload(string name, string payload)
        {
            DisplayName = name;
            Payload = payload;
        }
    }
}
