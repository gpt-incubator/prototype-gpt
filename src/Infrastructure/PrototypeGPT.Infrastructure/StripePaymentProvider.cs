using PrototypeGPT.Application.Providers;
using PrototypeGPT.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Stripe;

namespace PrototypeGPT.Infrastructure;

public class StripePaymentProvider : IPaymentProvider
{
    private readonly StripeOptions stripeOptions;

    public StripePaymentProvider(IOptions<StripeOptions> options)
    {
        stripeOptions = options.Value;
        StripeConfiguration.ApiKey = stripeOptions.SecretKey;
        PaymentIntentService = new PaymentIntentService();
    }

    public PaymentIntentService PaymentIntentService { get; init; }

    public async Task<string> CreateAsync(CancellationToken cancellationToken)
    {
        var paymentIntent = await PaymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = 8,
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        }, cancellationToken: cancellationToken);

        return paymentIntent.ClientSecret;
    }
}
