using System.ComponentModel.DataAnnotations;

namespace XinjingdailyBot.WebAPI.IPC.Data
{
    public sealed record NewPostData
    {
        public IFormFileCollection? Media { get; set; }
        [MaxLength(10)]
        public string? Text { get; set; }
        public string? From { get; set; }
    }
}
