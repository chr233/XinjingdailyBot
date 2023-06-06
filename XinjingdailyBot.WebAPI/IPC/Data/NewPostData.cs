namespace XinjingdailyBot.WebAPI.IPC.Data
{
    public sealed record NewPostData
    {
        public IFormFileCollection? Media { get; set; }
        public string? Text { get; set; }
        public string? From { get; set; }
    }
}
