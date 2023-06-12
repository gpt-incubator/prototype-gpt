using PrototypeGPT.Infrastructure.Identity.Entities;
using System.Security.Claims;

namespace PrototypeGPT.Infrastructure.Identity.Auth;

public class Tokens
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="roles"></param>
    /// <param name="jwtFactory"></param>
    /// <param name="jwtOptions"></param>
    /// <returns></returns>
    public static async Task<object> GenerateJwt(User user, IList<string> roles, IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName)
        };

        claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, string.Join(',', roles)));

        return new
        {
            user.UserName,
            roles,
            auth_token = await jwtFactory.GenerateEncodedToken(user.UserName, claims),
            expires_in = (int)jwtOptions.ValidFor.TotalSeconds
        };
    }
}
