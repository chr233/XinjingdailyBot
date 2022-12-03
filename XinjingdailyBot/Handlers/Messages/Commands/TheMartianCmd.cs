using XinjingdailyBot.Helpers;

namespace XinjingdailyBot.Handlers.Messages.Commands;

internal static class TheMartianCmd
{
    internal static async Task ResponseMars(ITelegramBotClient botClient, Users dbUser, Message message)
    {
        async Task<string> Executor(Posts? post)
        {
            if (post is null)
                return "无法找到指定稿件信息";
            if (post.PublicMsgID < 0)
                return "该稿件未通过审核";

            // var count = post.ReviewMsgID - post.OriginMsgID;
            // for (var i = 0; i < count; i++)
            // {
            //     var id = (int)post.PublicMsgID + i;
            //     await botClient.DeleteMessageAsync(Utils.AcceptChannel.Id, id);
            // }
            
            // TODO: 获取消息并删除

            return "执行完成";
        }

        var replyCmdMsg = string.Empty;

        var msgText = message.Text;
        if (msgText is null or "")
        {
            var repMsg = message.ReplyToMessage;
            if (repMsg is not null)
            {
                var msgId = repMsg.MessageId;
                var post = await DataBaseHelper.DB.Queryable<Posts>().FirstAsync(e => e.ActionMsgID == msgId);
                replyCmdMsg = await Executor(post);
            }
            else
                replyCmdMsg = "请回复投稿信息或是指定信息id";
        }
        else
        {
            var args = msgText.Split(' ');
            if (args.Length > 1)
            {
                var id = args[1];
                if (long.TryParse(id, out var msgId))
                {
                    var post = await DataBaseHelper.DB.Queryable<Posts>().FirstAsync(e => e.ActionMsgID == msgId);
                    replyCmdMsg = await Executor(post);
                }
                else
                    replyCmdMsg = $"无法解析指定稿件信息id: {id}";
            }
            else
                replyCmdMsg = "无法找到指定稿件信息";
        }

        await botClient.SendCommandReply(replyCmdMsg, message, false);
    }
}
