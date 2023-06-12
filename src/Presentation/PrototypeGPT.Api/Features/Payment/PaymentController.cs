using PrototypeGPT.Api.Features.Auth;
using PrototypeGPT.Api.Features.Payment.Services;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrototypeGPT.Api.Features.Payment;

[Authorize]
[Produces("application/json")]
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly ILogger<AuthController> logger;
    private readonly IPaymentApiService paymentApiService;

    public PaymentController(ILogger<AuthController> logger, IPaymentApiService paymentApiService)
    {
        this.logger = logger;
        this.paymentApiService = paymentApiService;
    }

    [HttpPost("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        logger.LogInformation("Create payment intent");
        var result = await paymentApiService.CreateAsync(cancellationToken);        
        if (!result.IsSuccess)
        {
            return result.ToActionResult();
        }

        return Ok(new { clientSecret = result.Value });
    }
}
