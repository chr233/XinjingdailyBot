using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IReviewVoteService), LifeTime.Transient)]
    public sealed class ReviewVoteService : BaseService<ReviewVotes>, IReviewVoteService
    {
        //public async 
    }
}
