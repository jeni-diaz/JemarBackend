using System;
using System.Security.Claims;

namespace Jemar.Presentation.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? context.User.FindFirst("userId")?.Value;

                Guid userId = Guid.TryParse(userIdValue, out var result)
                    ? result
                    : Guid.Empty;

                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value
                               ?? context.User.FindFirst("role")?.Value
                               ?? string.Empty;

                context.Items["UserId"] = userId;
                context.Items["UserRole"] = userRole;
            }

            await _next(context);
        }
    }
}