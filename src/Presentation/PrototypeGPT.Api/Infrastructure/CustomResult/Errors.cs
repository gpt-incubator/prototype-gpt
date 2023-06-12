using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace PrototypeGPT.Api.Infrastructure.CustomResult;

public abstract class ApiProblemDetailsError : Error
{
    public ValidationProblemDetails ProblemDetails { get; }

    protected ApiProblemDetailsError(HttpContext httpContext, HttpStatusCode statusCode, string title, IDictionary<string, string[]> errors)
        : base(title)
    {
        ProblemDetails = new ValidationProblemDetails(errors)
        {
            Title = title,
            Status = (int)statusCode,
            Type = $"https://httpstatuses.io/{(int)statusCode}",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
            },
        };
    }
}

public class InvalidUserError : ApiProblemDetailsError
{
    public InvalidUserError(HttpContext httpContext, IEnumerable<IdentityError> errors)
        : base(httpContext, HttpStatusCode.BadRequest, "Invalid User", CreateErrorDictionary(errors))
    { }

    private static IDictionary<string, string[]> CreateErrorDictionary(IEnumerable<IdentityError> identityErrors)
    {
        var errorDictionary = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var identityError in identityErrors)
        {
            var key = identityError.Code;
            var error = identityError.Description;

            errorDictionary.Add(key, error.Split(','));
        }

        return errorDictionary;
    }
}

public class UnauthorizedError : ApiProblemDetailsError
{
    public UnauthorizedError(HttpContext httpContext, string username, string resource)
        : base(httpContext, HttpStatusCode.Unauthorized, "Unauthorized", CreateErrorDictionary(username, resource))
    {
    }

    private static IDictionary<string, string[]> CreateErrorDictionary(string username, string resource)
    {
        var errorDictionary = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { username, new[] { $"is not authorized to access {resource}." } }
        };

        return errorDictionary;
    }
}

public class NotFoundError : ApiProblemDetailsError
{
    public NotFoundError(HttpContext httpContext, string entityName)
        : base(httpContext, HttpStatusCode.NotFound, "Not Found", CreateErrorDictionary(entityName))
    {
    }

    private static IDictionary<string, string[]> CreateErrorDictionary(string entityName)
    {
        var errorDictionary = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { entityName, new[] { $"'{entityName}' not found." } }
        };

        return errorDictionary;
    }
}

public class BadRequestError : ApiProblemDetailsError
{
    public BadRequestError(HttpContext httpContext, string message)
        : base(httpContext, HttpStatusCode.NotFound, "Not Found", CreateErrorDictionary(message))
    {
    }

    private static IDictionary<string, string[]> CreateErrorDictionary(string message)
    {
        var errorDictionary = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { "Detail", new[] { message } }
        };

        return errorDictionary;
    }
}
