using PrototypeGPT.Application.Providers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PrototypeGPT.Infrastructure.Identity.Auth;

/// <summary>
/// 
/// </summary>
public class AuthProvider : IAuthProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/></param>
    public AuthProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string UserName()
    {
        return httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] Roles()
    {
        var roles = httpContextAccessor?.HttpContext?.User?.Claims?.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType)?.FirstOrDefault()?.Value ?? string.Empty;

        if (roles.Contains(","))
        {
            return roles.Split(",");
        }

        return new string[] { roles };
    }
}
