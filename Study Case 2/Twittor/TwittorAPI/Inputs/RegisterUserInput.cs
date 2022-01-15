using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwittorAPI.Input
{
    public record RegisterUserInput
    (
        string FullName,
        string Email,
        string UserName,
        string? Password
    );
}