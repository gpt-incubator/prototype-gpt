using FluentResults;

namespace PrototypeGPT.Api.Features.Auth.Services;

public interface IAuthApiService
{
    /// <summary>
    /// Sign Up a user that does not exist
    /// </summary>
    /// <param name="email"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Result> SignUpAsync(string email, string userName, string password);

    /// <summary>
    /// Sign In a registered user
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public Task<Result<object>> SignInAsync(string userName, string password);
}
