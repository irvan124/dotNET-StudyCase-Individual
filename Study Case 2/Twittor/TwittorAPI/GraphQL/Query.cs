using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TwittorAPI.Kafka;
using TwittorAPI.Models;

namespace TwittorAPI.GraphQL
{
    public class Query
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Query(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IQueryable<Tweet>> GetTweetsAsync([Service] TwittorDBContext context, [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var tweets = context.Tweets;

            var key = "Getting-tweets -->" + DateTime.Now.ToString();
            var val = "Get All Tweets";
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            return tweets;
        }

        public async Task<IQueryable<User>> GetProfileUserAsync([Service] TwittorDBContext context, [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            // if(currentUserId == null)
            // {
            //     return await Task.FromResult(new DataTransferStatus(false, "User is exists"));
            // }
            var user = context.Users.Where(u => u.UserId == Convert.ToInt32(currentUserId));

            var key = "Getting-userprofile -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(user.SingleOrDefault()).ToString(Formatting.None);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);
            return user;
        }

        public IQueryable<Role> GetRolesAsync([Service] TwittorDBContext context) =>
        context.Roles;

        public IQueryable<UserRole> GetUserRolesAsync([Service] TwittorDBContext context) =>
       context.UserRoles;
    }
}
