using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IAttachmentService), LifeTime.Transient)]
    public sealed class AttachmentService : BaseService<Attachments>, IAttachmentService
    {
    }
}
