using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwittorAPI.Input
{
    public record LoginUserInput
   (
       string Username,
       string Password
   );
}