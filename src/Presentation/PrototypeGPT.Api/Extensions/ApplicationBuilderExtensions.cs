using PrototypeGPT.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace PrototypeGPT.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(options =>
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var ex = context.Features.Get<IExceptionHandlerFeature>();
                if (ex != null)
                {
                    string response = new ProblemDetails
                    {
                        Title = "Internal Server Error",
                        Status = context.Response.StatusCode
                    }.ToString()!;

                    await context.Response.WriteAsync(response);
                }
            });
        });
    }

    public static void ConfigureForwardedHeaders(this IApplicationBuilder app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
    }

    public static void UpdateDatabase(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PrototypeGPTContext>();
            db.Database.Migrate();
        }
    }

    public static void ConfigureCors(this IApplicationBuilder app)
    {
        app.UseCors(x => x
            .WithOrigins("https://nginx")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    }
}
