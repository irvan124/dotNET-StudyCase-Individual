using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Mutation(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<DataTransferStatus> EditProfileAsync(
            RegisterUserInput input,
            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");

            var user = new User();
            user = context.Users.Where(u => u.UserId == Convert.ToInt32(currentUserId)).SingleOrDefault();

            user.FullName = input.FullName;
            user.Email = input.Email;
            user.Username = input.UserName;

            var key = "Edit-Profile -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "edit-user", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Edit profile Success");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to edit profile");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "MEMBER", "ADMIN" })]
        public async Task<DataTransferStatus> ChangePasswordAsync(
            ChangePasswordInput input,

            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");

            var user = new User();
            user = context.Users.Where(u => u.UserId == Convert.ToInt32(currentUserId)).SingleOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User is not Exist"));
            }
            if (user.IsLocked == true)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User Locked"));
            }

            var valid = BCrypt.Net.BCrypt.Verify(input.currentPassword, user.Password);
            if (valid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.newPassword);
            }
            else return new DataTransferStatus(false, "Password is incorrect");

            var key = "Change-Password -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "edit-user", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Password successfuly updated");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to update the Password");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<DataTransferStatus> LockUserAsync(
            LockUserInput input,

            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var user = context.Users.Where(u => u.UserId == input.UserId).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User is not found"));
            }

            user.IsLocked = input.IsLocked;

            var key = "Lock-user -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "edit-user", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Lock privilage succesfully updated");
            if (!result)
                ret = new DataTransferStatus(result, "Failed update User Lock privilage");

            return await Task.FromResult(ret);

        }

        public async Task<DataTransferStatus> CreateRoleAsync(
            CreateRoleInput input,
            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var role = context.Roles.Where(r => r.Name.ToUpper() == input.Name.ToUpper()).SingleOrDefault();
            if (role == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "Rolename is already exist"));
            }

            var newRolename = new Role { Name = input.Name };

            var key = "Add-rolename -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(newRolename).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "add-rolename", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Rolename successfuly added");
            if (!result)
                ret = new DataTransferStatus(result, "Failed Add new Rolename");

            return await Task.FromResult(ret);

        }
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<DataTransferStatus> ChangeUserRoleAsync(
            ChangeUserRoleInput input,
            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var user = context.Users.Where(u => u.UserId == input.UserId).SingleOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User not Found"));
            }
            var role = context.Roles.Where(r => r.RoleId == input.RoleId).SingleOrDefault();
            if (role == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "Role not Found"));
            }
            var UserRole = context.UserRoles.Where(ur => ur.UserId == input.UserId && ur.RoleId == input.RoleId).SingleOrDefault();
            if (UserRole != null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "UserRole is already Exist"));
            }

            var newUserRole = new UserRole
            {
                RoleId = role.RoleId,
                UserId = user.UserId
            };

            var key = "Update-userRole -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUserRole).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "update-userRole", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "UserRole successfuly added");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to Update UserRole");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<DataTransferStatus> PostTwtitAsync(
            CreateTwitInput input,
            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            var user = context.Users.Where(u => u.UserId == Convert.ToInt32(currentUserId)).SingleOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User is not Exist"));
            }
            if (user.IsLocked == true)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User Locked"));
            }
            // Console.WriteLine(currentUserId);
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

        [Authorize(Roles = new[] { "MEMBER" })]
        public async Task<DataTransferStatus> CommentTwitAsync(
            CreateCommentInput input,
            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            var user = context.Users.Where(user => user.UserId == Convert.ToInt32(currentUserId)).SingleOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User is not Exist"));
            }
            if (user.IsLocked == true)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User Locked"));
            }
            var tweetId = context.Tweets.Where(tw => tw.TweetId == input.TweetId).SingleOrDefault();
            if (tweetId == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "Tweet is not Exist"));
            }

            var comment = new Comment
            {
                UserId = Convert.ToInt32(currentUserId),
                TweetId = input.TweetId,
                Text = input.Text,
                CreatedAt = DateTime.Now
            };

            var key = "Comment-Twit -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(comment).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "comment-twit", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Comment tweet Success");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to Comment the Tweet Data");

            return await Task.FromResult(ret);
        }

        public async Task<DataTransferStatus> DeleteTwitAsync(
            DeleteTwitInput input,
            [Service] TwittorDBContext context,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings
        )
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue("UserId");
            var user = context.Users.Where(u => u.UserId == Convert.ToInt32(currentUserId)).SingleOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User is not Exist"));
            }
            if (user.IsLocked == true)
            {
                return await Task.FromResult(new DataTransferStatus(false, "User Locked"));
            }
            var tweet = context.Tweets.Where(tw => tw.TweetId == input.TweetId).SingleOrDefault();
            if (tweet == null)
            {
                return await Task.FromResult(new DataTransferStatus(false, "Tweet is not Exist"));
            }

            var key = "Delete-Twit -->" + DateTime.Now.ToString();
            var val = JObject.FromObject(tweet).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "delete-twit", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new DataTransferStatus(result, "Delete tweet Success");
            if (!result)
                ret = new DataTransferStatus(result, "Failed to Delete the Tweet Data");

            return await Task.FromResult(ret);
        }
    }
}
