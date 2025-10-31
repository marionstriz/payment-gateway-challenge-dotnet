using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models;

public record Payment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required PaymentStatus Status { get; init; }
    
    public required string CardNumberLastFour { get; init; }
    
    public required int ExpiryMonth { get; init; }
    
    public required int ExpiryYear { get; init; }
    
    public required CurrencyCode Currency { get; init; }
    
    public required int Amount { get; init; }
}