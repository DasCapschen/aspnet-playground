using System;
using System.Security.Claims;

namespace src.Extensions
{
    public static class PrincipalExtensions
    {
        public static string GetCulture(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue("src:Culture");
        }
        public static string GetTimeZone(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue("src:TimeZone");
        }
    }
}