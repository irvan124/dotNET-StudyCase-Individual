using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwittorAPI.Inputs
{
    public record ChangeUserRoleInput(int UserId, int RoleId);
}