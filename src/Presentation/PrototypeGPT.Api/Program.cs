using PrototypeGPT.Api.Extensions;
using PrototypeGPT.Api.Infrastructure.CustomResult.CustomResult;
using FluentResults.Extensions.AspNetCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using Serilog.Debugging;

SelfLog.Enable(Console.Error);

AspNetCoreResult.Setup(config => config.DefaultProfile = new CustomAspNetResultProblemDetailProfile());

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureLogging((_, loggingBuilder) => loggingBuilder.ClearProviders())
    .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// builder.Services.AddHttpClient(); // uncomment to inject IHttpClientFactory
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddRateLimiter()
    .AddDependencyInjection()
    .AddDbContext(builder.Environment, builder.Configuration)
    .AddCache(builder.Environment, builder.Configuration)
    .AddHealthChecks(builder.Environment, builder.Configuration)
    .AddJwtAuthentication(builder.Environment, builder.Configuration)
    .AddGptOptions(builder.Environment, builder.Configuration)
    .AddPaymentOptions(builder.Environment, builder.Configuration);

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.ConfigureExceptionHandler();
    app.UseHsts();
}

app.ConfigureCors();
app.ConfigureForwardedHeaders();

app.MapHealthChecks("/_health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthentication();

// This will create metrics for each HTTP endpoint, giving us 'number of requests', 'request times' etc 
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("host", context => context.Request.Host.Host);
    options.AddCustomLabel("logicalService", context => "my-service-name");
});

app.UseRouting().UseAuthorization().UseRateLimiter().UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
});

app.UpdateDatabase();

app.Run();

public partial class Program { }