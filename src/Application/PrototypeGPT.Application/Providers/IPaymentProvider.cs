namespace PrototypeGPT.Application.Providers;

public interface IPaymentProvider
{
    Task<string> CreateAsync(CancellationToken cancellationToken);
}
