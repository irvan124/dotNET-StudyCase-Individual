using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.DTO;
using AuthServer.Models;

namespace AuthServer.Data
{
    public interface IUser
    {
        IEnumerable<UserDto> GetAllusers();
        Task Registration(CreateUserDto input);
        Task AddRole(string rolename);
        IEnumerable<CreateRoleDto> GetAllRoles();
        Task AddUserToRole(string username, string role);
        Task<List<string>> GetRolesFromUser(string username);
        Task<User> Login(string username, string password);
    }
}