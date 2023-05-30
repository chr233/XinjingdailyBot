using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    [AppService(LifeTime.Transient)]
    [Obsolete("废弃的仓储类")]
    public class OldPostRepository : BaseRepository<OldPosts>
    {
    }
}
