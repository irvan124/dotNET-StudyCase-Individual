using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwittorAPI.Inputs
{
    public record LockUserInput(int UserId, bool IsLocked);
}