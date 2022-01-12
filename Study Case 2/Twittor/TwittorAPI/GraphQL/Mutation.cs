using HotChocolate;
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
using TwittorAPI.Kafka;
using TwittorAPI.Models;

namespace TwittorAPI.GraphQL
{

    public class Mutation
    {

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

            var ret = new DataTransferStatus(result, "");
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

                foreach (var userRole in user.UserRoles)
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
    }
}
