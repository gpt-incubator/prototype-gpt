using FluentResults;

namespace PrototypeGPT.Api.Features.Payment.Services;

public interface IPaymentApiService
{
    Task<Result<string>> CreateAsync(CancellationToken cancellationToken);
}
