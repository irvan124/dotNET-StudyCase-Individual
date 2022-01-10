using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Data;
using AuthServer.DTO;
using AuthServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUser _user;

        public UsersController(IUser user)
        {
            _user = user;
        }
        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> GetAll()
        {
            return Ok(_user.GetAllusers());
        }
        [HttpPost]
        public async Task<ActionResult> Registration(CreateUserDto input)
        {
            try
            {
                await _user.Registration(input);
                return Ok($"{input.UserName} is successfully registered!");
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        [HttpGet("Role")]
        public ActionResult GetAllRoles()
        {
            return Ok(_user.GetAllRoles());
        }
        [HttpPost("Role")]
        public async Task<ActionResult> AddRole(CreateRoleDto input)
        {
            try
            {
                await _user.AddRole(input.RoleName);
                return Ok($"{input.RoleName} is successfully added to the Role Lists!");
            }
            catch (System.Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        [HttpPost("AddRoleToUser")]
        public async Task<ActionResult> AddUserToRole(string username, string role)
        {
            try
            {
                await _user.AddUserToRole(username, role);
                return Ok($"{role} is added to the {username} account !");
            }
            catch (System.Exception ex)
            {

                throw new System.Exception(ex.Message);
            }
        }
        [HttpGet("RolesFromUser")]
        public async Task<ActionResult> GetRolesFromUser(string username)
        {
            var result = await _user.GetRolesFromUser(username);
            return Ok(result);
        }
        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login(LoginUserDto input)
        {
            try
            {
                var result = await _user.Login(input.UserName, input.Password);
                return Ok(result);

            }
            catch (System.Exception ex)
            {

                throw new System.Exception(ex.Message);
            }
        }
    }
}