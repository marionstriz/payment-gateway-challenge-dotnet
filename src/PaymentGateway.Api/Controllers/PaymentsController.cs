using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentsRepository paymentsRepository) : Controller
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var paymentDao = await paymentsRepository.GetAsync(id);
        
        if (paymentDao is null)
        {
            return NotFound();
        }

        var paymentResponse = new PaymentResponse
        {
            Id = paymentDao.Id,
            ExpiryMonth = paymentDao.ExpiryMonth,
            ExpiryYear = paymentDao.ExpiryYear,
            Amount = paymentDao.Amount,
            CardNumberLastFour = paymentDao.CardNumberLastFour,
            Currency = paymentDao.Currency,
            Status = paymentDao.Status
        };

        return new OkObjectResult(paymentResponse);
    }
}