using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models;

public record PaymentInfo
{
    public required string CardNumber { get; init; }

    public required int ExpiryMonth { get; init; }
    
    public required int ExpiryYear { get; init; }
    
    public required CurrencyCode Currency { get; init; }
    
    public required int Amount { get; init; }
    
    public required string Cvv { get; init; }
}