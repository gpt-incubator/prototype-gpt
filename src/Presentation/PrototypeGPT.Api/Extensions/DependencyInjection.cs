using PrototypeGPT.Api.Features.Auth.Services;
using PrototypeGPT.Api.Features.Payment.Services;
using PrototypeGPT.Application.DataAccess;
using PrototypeGPT.Application.Providers;
using PrototypeGPT.Infrastructure;
using PrototypeGPT.Infrastructure.DataAccess;
using PrototypeGPT.Infrastructure.Identity.Auth;

namespace PrototypeGPT.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        // Singletons
        services.AddSingleton<IJwtFactory, JwtFactory>();

        // Data Access
        services.AddScoped<IDbContext, PrototypeGPTContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Providers
        services.AddScoped<IAuthProvider, AuthProvider>();
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IPrompterGptProvider, PrompterGptProvider>();
        services.AddSingleton<ICacheProvider, CacheProvider>();

        // Services API
        services.AddScoped<IAuthApiService, AuthApiService>();
        services.AddScoped<IPaymentApiService, PaymentApiService>();

        // Services

        return services;
    }
}
