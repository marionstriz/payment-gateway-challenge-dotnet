using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentsRepository paymentsRepository, IBankClient bankClient) : Controller
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentResponse?>> ProcessPaymentAsync(
        [FromBody] ProcessPaymentRequest paymentRequest)
    {
        if (!ModelState.IsValid)
        {
            var rejectedResponse = new PaymentResponse
            {
                Id = Guid.NewGuid(),
                ExpiryMonth = paymentRequest.ExpiryMonth,
                ExpiryYear = paymentRequest.ExpiryYear,
                Amount = paymentRequest.Amount,
                CardNumberLastFour = paymentRequest.CardNumber[^4..],
                Currency = paymentRequest.Currency,
                Status = PaymentStatus.Rejected
            };
            return new BadRequestObjectResult(rejectedResponse);
        }
        
        var paymentInfo = CreatePaymentInfo(paymentRequest);
        var authorizationInfo = await bankClient.AuthorizePaymentAsync(paymentInfo);

        var paymentResponse = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Amount = paymentRequest.Amount,
            CardNumberLastFour = paymentRequest.CardNumber[^4..],
            Currency = paymentRequest.Currency,
            Status = authorizationInfo.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
        };

        return new OkObjectResult(paymentResponse);
    }

    private PaymentInfo CreatePaymentInfo(ProcessPaymentRequest paymentRequest)
    {
        var currencyCode = Enum.Parse<CurrencyCode>(paymentRequest.Currency.Trim().ToUpper());
        
        return new PaymentInfo
        {
            CardNumber = paymentRequest.CardNumber,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Amount = paymentRequest.Amount,
            Currency = currencyCode,
            Cvv = paymentRequest.Cvv
        };
    }
}