namespace XinjingdailyBot.Infrastructure.Model
{
    public sealed record TagObjct
    {
        private List<string> Content { get; }

        public TagObjct()
        {
            Content = new();
        }
        public TagObjct(string content)
        {
            Content = new()
            {
                content
            };
        }

        public void AddLast(string value)
        {
            Content.Add(value);
        }

        public void AddFirst(string value)
        {
            Content.Insert(0, value);
        }

        public override string? ToString()
        {
            return string.Join("", Content);
        }
    }
}
