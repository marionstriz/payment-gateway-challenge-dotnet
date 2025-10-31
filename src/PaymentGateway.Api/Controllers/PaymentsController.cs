using System.Net;

using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Api.Requests;
using PaymentGateway.Api.Models.Api.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class PaymentsController(
    IPaymentsRepository paymentsRepository, 
    IAuthorizer authorizer, 
    ILogger<PaymentsController> logger) : Controller
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        try
        {
            var payment = await paymentsRepository.GetAsync(id);

            if (payment is null)
            {
                return new NotFoundObjectResult(new ErrorResponse
                {
                    Message = $"Payment with ID {id} not found.", Code = StatusCodes.Status404NotFound
                });
            }

            var paymentResponse = BuildPaymentResponse(payment);

            return new OkObjectResult(paymentResponse);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured processing GET payment request: {Message}", e.Message);
            return BuildErrorResponseResult(HttpStatusCode.InternalServerError, 
                "Oops! That was not supposed to happen. Please contact the system administrator.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<PaymentResponse?>> ProcessPaymentAsync(
        [FromBody] ProcessPaymentRequest paymentRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                PaymentResponse rejectedResponse = BuildPaymentResponse(paymentRequest);
                return new BadRequestObjectResult(rejectedResponse);
            }

            var paymentInfo = BuildPaymentInfo(paymentRequest);
            var authorizationInfo = await authorizer.AuthorizePaymentAsync(paymentInfo);

            var paymentResponse = BuildPaymentResponse(paymentRequest, authorizationInfo);
            
            var payment = BuildPayment(paymentResponse);
            await paymentsRepository.AddAsync(payment);

            return new OkObjectResult(paymentResponse);
        }
        catch (PaymentAuthorizationFailedException e)
        {
            logger.LogWarning(e, "Payment authorization failed for payment request: {Message}", e.Message);
            return BuildErrorResponseResult(HttpStatusCode.ServiceUnavailable,
                "Payment service temporarily unavailable. Please try again later.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured processing POST payment request: {Message}", e.Message);
            return BuildErrorResponseResult(HttpStatusCode.InternalServerError, 
                "Oops! That was not supposed to happen. Please contact the system administrator.");
        }
    }

    private static PaymentResponse BuildPaymentResponse(ProcessPaymentRequest paymentRequest,
        PaymentAuthorizationResult? authorizationInfo = null)
    {
        PaymentStatus status;
        if (authorizationInfo is null)
        {
            status = PaymentStatus.Rejected;
        }
        else
        {
            status = authorizationInfo.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
        }
        var rejectedResponse = new PaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Amount = paymentRequest.Amount,
            CardNumberLastFour = paymentRequest.CardNumber[^4..],
            Currency = paymentRequest.Currency,
            Status = status.ToString()
        };
        return rejectedResponse;
    }
    
    private static PaymentResponse BuildPaymentResponse(Payment payment)
    {
        var paymentResponse = new PaymentResponse
        {
            Id = payment.Id,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Amount = payment.Amount,
            CardNumberLastFour = payment.CardNumberLastFour,
            Currency = payment.Currency.ToString(),
            Status = payment.Status.ToString()
        };
        return paymentResponse;
    }

    private PaymentRequest BuildPaymentInfo(ProcessPaymentRequest paymentRequest)
    {
        var currencyCode = Enum.Parse<CurrencyCode>(paymentRequest.Currency.Trim().ToUpper());
        
        return new PaymentRequest
        {
            CardNumber = paymentRequest.CardNumber,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Amount = paymentRequest.Amount,
            Currency = currencyCode,
            Cvv = paymentRequest.Cvv
        };
    }
    
    private Payment BuildPayment(PaymentResponse paymentResponse)
    {
        return new Payment
        {
            Id = paymentResponse.Id,
            ExpiryMonth = paymentResponse.ExpiryMonth,
            ExpiryYear = paymentResponse.ExpiryYear,
            Amount = paymentResponse.Amount,
            CardNumberLastFour = paymentResponse.CardNumberLastFour,
            Currency = Enum.Parse<CurrencyCode>(paymentResponse.Currency),
            Status = Enum.Parse<PaymentStatus>(paymentResponse.Status)
        };
    }

    private ObjectResult BuildErrorResponseResult(HttpStatusCode statusCode, string errorMessage)
    {
        return new ObjectResult(new ErrorResponse
        {
            Code = (int)statusCode,
            Message = errorMessage
        })
        {
            StatusCode = (int)statusCode
        };
    }
}