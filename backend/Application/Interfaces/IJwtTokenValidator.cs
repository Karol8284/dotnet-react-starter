using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    internal interface IJwtTokenValidator
    {
        public Task<ClaimsPrincipal> ValidateTokenAsync(string token);
    }
}
