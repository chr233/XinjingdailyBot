namespace XinjingdailyBot.Infrastructure.Model
{
    /// <summary>
    /// 消息Tag
    /// </summary>
    public sealed record TagObjct
    {
        private List<string> Content { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="content"></param>
        public TagObjct(string content)
        {
            Content = new() { content };
        }

        /// <summary>
        /// 插入到最后
        /// </summary>
        /// <param name="value"></param>
        public void AddLast(string value)
        {
            Content.Add(value);
        }

        /// <summary>
        /// 插入到最前
        /// </summary>
        /// <param name="value"></param>
        public void AddFirst(string value)
        {
            Content.Insert(0, value);
        }

        /// <summary>
        /// 文本显示
        /// </summary>
        /// <returns></returns>
        public override string? ToString()
        {
            return string.Join("", Content);
        }
    }
}
