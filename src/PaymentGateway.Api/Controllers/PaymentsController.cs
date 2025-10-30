using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.BankClients;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentsRepository paymentsRepository, IBankClient bankClient) : Controller
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
    
    [HttpPost]
    public async Task<ActionResult<PaymentResponse?>> ProcessPaymentAsync(
        [FromBody] ProcessPaymentRequest paymentRequest)
    {
        var authorizationResponse = await bankClient.AuthorizePaymentAsync(paymentRequest);

        var paymentResponse = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Amount = paymentRequest.Amount,
            CardNumberLastFour = paymentRequest.CardNumber[^4..],
            Currency = paymentRequest.Currency,
            Status = authorizationResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
        };

        return new OkObjectResult(paymentResponse);
    }
}