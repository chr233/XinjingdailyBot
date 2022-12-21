using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Data
{
    [AppService(ServiceType = typeof(IChannelOptionService), ServiceLifetime = LifeTime.Transient)]
    public sealed class CmdRecordService : BaseService<ChannelOptions>, ICmdRecordService
    {
        private readonly CmdRecordRepository _cmdRecordRepository;

        public CmdRecordService(CmdRecordRepository cmdRecordRepository)
        {
            _cmdRecordRepository = cmdRecordRepository;
        }


        /// <summary>
        /// 获取频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelName"></param>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        public async Task AddCmdRecord(Message message, Users dbUser, string command, bool handled, bool isQuery, string? exception = null)
        {
            bool error = !string.IsNullOrEmpty(exception);

            CmdRecords record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = command,
                Handled = handled,
                IsQuery = isQuery,
                Error = error,
                Exception = exception ?? "",
                ExecuteAt = DateTime.Now,
            };

            await _cmdRecordRepository.Insertable(record).ExecuteCommandAsync();
        }
    }
}
