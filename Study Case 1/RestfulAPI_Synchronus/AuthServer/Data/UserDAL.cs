using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthServer.DTO;
using AuthServer.Helpers;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Data
{
    public class UserDAL : IUser
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private AppSettings _appSettings;

        public UserDAL(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager, IOptions<AppSettings> appSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _appSettings = appSettings.Value;
        }

        public async Task AddRole(string rolename)
        {
            IdentityResult roleResult;

            try
            {
                var roleIsExist = await _roleManager.RoleExistsAsync(rolename);
                if (!roleIsExist)
                {
                    roleResult = await _roleManager.CreateAsync(new IdentityRole(rolename));
                }

            }
            catch (System.Exception)
            {

                throw new Exception($"Role {rolename} already Exists");
            }
        }

        public async Task AddUserToRole(string username, string role)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new Exception("Username was not found");
            }

            try
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public IEnumerable<CreateRoleDto> GetAllRoles()
        {
            List<CreateRoleDto> listRole = new List<CreateRoleDto>();
            var results = _roleManager.Roles;

            foreach (var role in results)
            {
                listRole.Add(new CreateRoleDto { RoleName = role.Name });
            }

            return listRole;
        }

        public IEnumerable<UserDto> GetAllusers()
        {
            List<UserDto> users = new List<UserDto>();
            var results = _userManager.Users;

            foreach (var user in results)
            {
                users.Add(new UserDto { UserName = user.UserName, Email = user.Email });
            }

            return users;
        }

        public async Task<List<string>> GetRolesFromUser(string username)
        {
            List<string> listRoles = new List<string>();

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new Exception($"{username} does not exist");
            }
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                listRoles.Add(role);
            }
            return listRoles;

        }

        public async Task<User> Login(string username, string password)
        {
            var userFind = await _userManager.CheckPasswordAsync(await _userManager.FindByNameAsync(username), password);
            if (!userFind)
            {
                var result = new User
                {
                    Message = "Incorrect Username or Password",
                    Token = ""
                };

                return result;
            }

            var user = new User
            {
                Message = "Login success",
            };

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, username));


            var roles = await GetRolesFromUser(username);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //Create Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // Durasi token aktif 1 jam
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
             SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user;
        }

        public async Task Registration(CreateUserDto input)
        {
            try
            {
                var newUser = new IdentityUser
                {
                    UserName = input.UserName,
                    Email = input.Email
                };

                var result = await _userManager.CreateAsync(newUser, input.Password);
                if (!result.Succeeded)
                    throw new Exception($"{result}");
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"Error: {ex.Message}");
            }
        }
    }
}