using Microsoft.AspNetCore.Http;

namespace RaceDay.Api.Helpers
{
    public static class SessionAuth
    {
        public static int? GetUserId(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32("UserId");
        }

        public static string? GetRole(HttpContext httpContext)
        {
            return httpContext.Session.GetString("Role");
        }

        public static bool IsLoggedIn(HttpContext httpContext)
        {
            return GetUserId(httpContext).HasValue;
        }

        public static bool HasRole(HttpContext httpContext, string role)
        {
            return GetRole(httpContext) == role;
        }
    }
}
