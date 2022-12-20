using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PostRepository : BaseRepository<Posts>
    {

    }
}
