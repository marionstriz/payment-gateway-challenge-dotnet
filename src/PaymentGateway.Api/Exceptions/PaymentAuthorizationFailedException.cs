using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Exceptions;

public class PaymentAuthorizationFailedException(string message, PaymentInfo paymentInfo) : Exception(message)
{
    public PaymentInfo PaymentInfo { get; } = paymentInfo;
}