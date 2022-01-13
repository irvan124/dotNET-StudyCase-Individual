using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TwittorAPI.Input;
using TwittorAPI.Inputs;
using TwittorAPI.Kafka;
using TwittorAPI.Models;

namespace TwittorAPI.GraphQL
{

    public class Mutation
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public Mutation(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public async Task<DataTransferStatus> RegisterUserAsync(
           RegisterUserInput input,
           [Service] TwittorDBContext context,
           [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User is exists"));
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
                IsLocked = false
            };

            var key = "register-user -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUser).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "add-user", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Register Success");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to Submit Data");

            return await Task.FromResult(ret);
        }


        public async Task<UserToken> LoginAsync(
          LoginUserInput input,

           [Service] IOptions<TokenSettings> tokenSettings,
           [Service] TwittorDBContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool IsPasswordValid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (IsPasswordValid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
                claims.Add(new Claim("UserId", user.UserId.ToString()));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.UserId).ToList();

                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.RoleId == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims,
                    signingCredentials: credentials
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }
        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<DataTransferStatus> PostTwtitAsync(
            CreateTwitInput input,

            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var currentUserId = _contextAccessor.HttpContext.User.FindFirstValue("UserId");
            Console.WriteLine(currentUserId);

            var twit = new Tweet
            {
                UserId = Convert.ToInt32(currentUserId),
                Text = input.Text,
                CreatedAt = DateTime.Now
            };



            var key = "Posting-Twit -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(twit).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "post-twit", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Posting twit Success");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to Submit Data");

            return await Task.FromResult(ret);
        }
    }
}
