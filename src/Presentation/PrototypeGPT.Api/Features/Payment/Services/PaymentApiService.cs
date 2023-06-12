using PrototypeGPT.Application.Providers;
using FluentResults;
using Stripe;

namespace PrototypeGPT.Api.Features.Payment.Services;

public class PaymentApiService : IPaymentApiService
{
    private readonly IPaymentProvider paymentProvider;

    public PaymentApiService(IPaymentProvider paymentProvider)
    {
        this.paymentProvider = paymentProvider;
    }

    /// <inheritdoc />
    public async Task<Result<string>> CreateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return Result.Ok(await paymentProvider.CreateAsync(cancellationToken));
        }
        catch (StripeException e)
        {
            return Result.Fail(e.Message);
        }
    }
}
