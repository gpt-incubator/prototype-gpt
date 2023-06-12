// File: C:\Users\ackse\source\repos\PrototypeGPT\src\Presentation\PrototypeGPT.Api\Extensions\ServiceExtensions.cs
using PrototypeGPT.Infrastructure.Configurations;
using PrototypeGPT.Infrastructure.DataAccess;
using PrototypeGPT.Infrastructure.Identity.Auth;
using PrototypeGPT.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Stripe;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace PrototypeGPT.Api.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("Anonymous", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter
            (
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                })
            );

            options.OnRejected = async (context, token) =>
            {
                int statusCode = (int)HttpStatusCode.TooManyRequests;
                context.HttpContext.Response.StatusCode = statusCode;
                var problemDetails = new ProblemDetails
                {
                    Type = $"https://httpstatuses.io/{statusCode}",
                    Title = "Too many requests.",
                    Status = (int)HttpStatusCode.TooManyRequests,
                    Instance = context.HttpContext.Request.Path,
                    Extensions =
                    {
                        ["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
                    },
                    Detail = "Too many requests. Please try again later."
                };

                var jsonProblemDetails = JsonSerializer.Serialize(problemDetails);
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

                    problemDetails.Detail = $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s).";
                }
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(jsonProblemDetails, cancellationToken: token);
            };
        });

        return services;
    }

    public static IServiceCollection AddDbContext(this IServiceCollection services, IWebHostEnvironment env, ConfigurationManager configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");

        if (!env.IsDevelopment())
        {
            var pgDatabase = Environment.GetEnvironmentVariable("PostgresDb") ??
                             Environment.GetEnvironmentVariable("PostgresDb", EnvironmentVariableTarget.User);
            var pgUserName = Environment.GetEnvironmentVariable("PostgresUser") ??
                             Environment.GetEnvironmentVariable("PostgresUser", EnvironmentVariableTarget.User);
            var pgpassword = Environment.GetEnvironmentVariable("PostgresPassword") ??
                             Environment.GetEnvironmentVariable("PostgresPassword", EnvironmentVariableTarget.User);

            connectionString = connectionString.Replace("{{database}}", pgDatabase)
                                               .Replace("{{username}}", pgUserName)
                                               .Replace("{{password}}", pgpassword);
        }

        services.AddDbContext<PrototypeGPTContext>
        (
            options => options
                .UseNpgsql(connectionString)
                .LogTo(Console.WriteLine)
        );

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services, IWebHostEnvironment env, ConfigurationManager configuration)
    {
        var connectionString = configuration.GetConnectionString("RedisConnection");

        if (!env.IsDevelopment())
        {
            var redisPassword = Environment.GetEnvironmentVariable("RedisPassword") ??
                               Environment.GetEnvironmentVariable("RedisPassword", EnvironmentVariableTarget.User);

            connectionString = connectionString.Replace("{{password}}", redisPassword);
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "master";
        });

        return services;
    }
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IWebHostEnvironment env, ConfigurationManager configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnection");

        if (!env.IsDevelopment())
        {
            var pgDatabase = Environment.GetEnvironmentVariable("PostgresDb") ??
                             Environment.GetEnvironmentVariable("PostgresDb", EnvironmentVariableTarget.User);
            var pgUserName = Environment.GetEnvironmentVariable("PostgresUser") ??
                             Environment.GetEnvironmentVariable("PostgresUser", EnvironmentVariableTarget.User);
            var pgpassword = Environment.GetEnvironmentVariable("PostgresPassword") ??
                             Environment.GetEnvironmentVariable("PostgresPassword", EnvironmentVariableTarget.User);

            connectionString = connectionString.Replace("{{database}}", pgDatabase)
                                               .Replace("{{username}}", pgUserName)
                                               .Replace("{{password}}", pgpassword);
        }

        var redisConnectionString = configuration.GetConnectionString("RedisConnection");

        if (!env.IsDevelopment())
        {
            var redisPassword = Environment.GetEnvironmentVariable("RedisPassword") ??
                               Environment.GetEnvironmentVariable("RedisPassword", EnvironmentVariableTarget.User);

            redisConnectionString = redisConnectionString.Replace("{{password}}", redisPassword);
        }

        services.AddHealthChecks()
            .AddNpgSql
            (
                connectionString,
                name: "postgres",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "postgres", "db" }
            )
            .AddRedis
            (
                redisConnectionString,
                name: "redis", 
                failureStatus: HealthStatus.Degraded, 
                tags: new[] { "redis", "cache" }
            )  
            .ForwardToPrometheus();

        return services;
    }
    
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IWebHostEnvironment env, ConfigurationManager configuration)
    {
        services.AddIdentity<User, Role>()
           .AddEntityFrameworkStores<PrototypeGPTContext>()
           .AddSignInManager()
           .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            options.User.RequireUniqueEmail = true;
        });

        // jwt wire up
        // Get options from app settings       

        var jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));
        var secretKey = configuration.GetValue<string>("SecurityKey");

        if (!env.IsDevelopment())
        {
            secretKey = Environment.GetEnvironmentVariable("SecurityKey") ??
                        Environment.GetEnvironmentVariable("SecurityKey", EnvironmentVariableTarget.User);
        }

        SymmetricSecurityKey _signingKey = new(Encoding.ASCII.GetBytes(secretKey));

        // Configure JwtIssuerOptions
        services.Configure<JwtIssuerOptions>(options =>
        {
            options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
            options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
            options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

            ValidateAudience = true,
            ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,

            RequireExpirationTime = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(configureOptions =>
        {
            configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
            configureOptions.TokenValidationParameters = tokenValidationParameters;
        });

        return services;
    }

    public static IServiceCollection AddGptOptions(this IServiceCollection services, IWebHostEnvironment env, ConfigurationManager configuration)
    {
        if (!env.IsDevelopment())
        {
            var apiKey = Environment.GetEnvironmentVariable("ApiKey") ??
                        Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.User);
            services.Configure<GptOptions>(option =>
            {
                option.ApiKey = apiKey;
            });
        }
        else
        {
            services.Configure<GptOptions>(configuration.GetSection(nameof(GptOptions)));
        }

        return services;
    }

    public static IServiceCollection AddPaymentOptions(this IServiceCollection services, IWebHostEnvironment env, ConfigurationManager configuration)
    {
        if (!env.IsDevelopment())
        {
            var secretKey = Environment.GetEnvironmentVariable("SecretKey") ??
                        Environment.GetEnvironmentVariable("SecretKey", EnvironmentVariableTarget.User);

            var publishableKey = Environment.GetEnvironmentVariable("PublishableKey") ??
                        Environment.GetEnvironmentVariable("PublishableKey", EnvironmentVariableTarget.User);

            services.Configure<StripeOptions>(option =>
            {
                option.SecretKey = secretKey;
                option.PublishableKey = publishableKey;
            });
        }
        else
        {
            services.Configure<StripeOptions>(configuration.GetSection(nameof(StripeOptions)));
        }        

        return services;
    }
}
