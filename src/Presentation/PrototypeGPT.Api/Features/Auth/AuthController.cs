using PrototypeGPT.Api.Features.Auth.Dtos;
using PrototypeGPT.Api.Features.Auth.Services;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace PrototypeGPT.Api.Features.Auth;

[EnableRateLimiting("Anonymous")]
[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> logger;
    private readonly IAuthApiService authApiService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController" /> class
    /// </summary>
    /// <param name="logger"><see cref="ILogger{AuthController}"/></param>
    /// <param name="authApiService"><see cref="IAuthApiService"/></param>
    public AuthController(ILogger<AuthController> logger, IAuthApiService authApiService)
    {
        this.logger = logger;
        this.authApiService = authApiService;        
    }

    /// <summary>
    /// Sign Up a user that does not exist
    /// </summary>
    /// <param name="signUpDto"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] SignUpDto signUpDto)
    {        
        var result = await authApiService.SignUpAsync(signUpDto.Email, signUpDto.UserName, signUpDto.Password);

        logger.LogInformation("Sign Up {@login}", signUpDto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Do the login.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST api/auth/login
    ///     {
    ///         "userName": "myusername",
    ///         "password": "mypassword"
    ///     }
    ///
    /// </remarks>
    /// <param name="signInDto">The credentials</param>
    /// <returns>The JWT</returns>
    /// <response code="200">Returns the JWT</response>
    /// <response code="400">Bad Request if the credentials are not correct</response>
    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignIn([FromBody] SignInDto signInDto)
    {        
        var result = await authApiService.SignInAsync(signInDto.UserName, signInDto.Password);

        logger.LogInformation("Sign In {@login}", signInDto);
        return result.ToActionResult();
    }
}