using PrototypeGPT.Api.Infrastructure.CustomResult;
using PrototypeGPT.Infrastructure.Identity.Auth;
using PrototypeGPT.Infrastructure.Identity.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace PrototypeGPT.Api.Features.Auth.Services;

/// <summary>
/// This class is responsible for handling the business logic for the Auth feature
/// </summary>
public class AuthApiService : IAuthApiService
{
    private readonly IJwtFactory jwtFactory;
    private readonly JwtIssuerOptions jwtOptions;
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController" /> class
    /// </summary>
    /// <param name="jwtFactory"><see cref="IJwtFactory"/></param>
    /// <param name="jwtOptions"><see cref="IOptions{JwtIssuerOptions}"/></param>
    /// <param name="userManager"><see cref="UserManager{User}"/></param>
    /// <param name="signInManager"><see cref="SignInManager{User}"/></param>
    public AuthApiService(IJwtFactory jwtFactory,
        IOptions<JwtIssuerOptions> jwtOptions,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        this.jwtFactory = jwtFactory;
        this.jwtOptions = jwtOptions.Value;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<Result> SignUpAsync(string email, string userName, string password)
    {
        var user = new User
        {
            Email = email,
            UserName = userName,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result.Fail(new InvalidUserError(httpContextAccessor.HttpContext!, result.Errors));
        }

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result<object>> SignInAsync(string userName, string password)
    {
        var user = await InternalSignInAsync(userName, password);
        if (user is null)
        {
            return Result.Fail(new BadRequestError(httpContextAccessor.HttpContext!, "Invalid username or password."));
        }

        var roles = await signInManager.UserManager.GetRolesAsync(user);

        var jwt = await Tokens.GenerateJwt(
            user,
            roles,
            jwtFactory,
            jwtOptions);

        return Result.Ok(jwt);

    }

    /// <summary>
    /// This method is responsible for signing in a user
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="password">The password</param>
    /// <returns>The user</returns>
    private async Task<User?> InternalSignInAsync(string userName, string password)
    {
        var result = await signInManager.PasswordSignInAsync(userName, password, false, false);
        if (result.Succeeded)
        {
            return await signInManager.UserManager.FindByNameAsync(userName);
        }

        return null;
    }
}
