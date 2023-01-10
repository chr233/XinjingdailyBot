namespace XinjingdailyBot.Infrastructure.Attribute
{

    /// <summary>
    /// 用于标记Query命令
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class TaskMarkAttribute : System.Attribute
    {
        //TimeSpan

        ///// <summary>
        ///// 创建特性
        ///// </summary>
        ///// <param name="command"></param>
        //public TaskMarkAttribute(string command)
        //{
        //    Command = command;
        //    Rights = UserRights.None;
        //}
        ///// <summary>
        ///// 创建特性
        ///// </summary>
        ///// <param name="command"></param>
        ///// <param name="rights"></param>
        //public TaskMarkAttribute(string command, UserRights rights)
        //{
        //    Command = command;
        //    Rights = rights;
        //}
    }
}
