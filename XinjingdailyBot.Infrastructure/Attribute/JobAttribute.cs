namespace XinjingdailyBot.Infrastructure.Attribute
{
    /// <summary>
    /// 用于标记定时任务
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class JobAttribute : System.Attribute
    {
        public string Schedule { get; set; }
        public string? Group { get; set; }

        public JobAttribute(string schedule)
        {
            Schedule = schedule;
        }

        public JobAttribute(string schedule, string group)
        {
            Schedule = schedule;
            Group = group;
        }
    }
}
