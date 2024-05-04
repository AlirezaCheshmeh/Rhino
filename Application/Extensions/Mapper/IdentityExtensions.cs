using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Application.Extensions.Mapper
{
    public static class IdentityExtensions
    {
        public static long GetUserId(this ClaimsPrincipal identity) => Convert.ToInt64(identity.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}
