using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PrototypeGPT.Api.Infrastructure.CustomResult.CustomResult
{
    public class CustomAspNetResultProblemDetailProfile : DefaultAspNetCoreResultEndpointProfile
    {
        public override ActionResult TransformFailedResultToActionResult(FailedResultToActionResultTransformationContext context)
        {
            var result = context.Result;

            if (result.HasError<ApiProblemDetailsError>(out var domainErrors))
            {
                var problemDetail = domainErrors.First().ProblemDetails;

                return (HttpStatusCode)problemDetail.Status! switch
                {
                    HttpStatusCode.NotFound => new NotFoundObjectResult(problemDetail),
                    HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(problemDetail),
                    HttpStatusCode.BadRequest => new BadRequestObjectResult(problemDetail),
                    HttpStatusCode.Conflict => new ConflictObjectResult(problemDetail),
                    HttpStatusCode.UnprocessableEntity => new UnprocessableEntityResult(),
                    _ => new StatusCodeResult((int)problemDetail.Status)
                };
            }

            return new StatusCodeResult(500);
        }
    }
}
