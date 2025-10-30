using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentsRepository paymentsRepository) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);
        
        if (payment is null)
        {
            return NotFound();
        }

        return new OkObjectResult(payment);
    }
}