namespace PaymentGateway.Api.Exceptions;

public class PaymentAuthorizationFailedException(string message) : Exception(message);