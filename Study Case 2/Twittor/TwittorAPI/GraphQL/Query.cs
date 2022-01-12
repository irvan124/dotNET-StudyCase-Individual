using HotChocolate;
using System.Linq;
using TwittorAPI.Models;

namespace TwittorAPI.GraphQL
{
    public class Query
    {
        public IQueryable<Tweet> GetTweets([Service] TwittorDBContext context) =>
            context.Tweets;
    }
}
