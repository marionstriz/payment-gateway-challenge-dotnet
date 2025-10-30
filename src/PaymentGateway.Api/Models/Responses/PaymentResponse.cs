using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses;

public record PaymentResponse
{
    public required Guid Id { get; init; } = Guid.NewGuid();
    
    public required string Status { get; init; }
    
    public required string CardNumberLastFour { get; init; }
    
    public required int ExpiryMonth { get; init; }
    
    public required int ExpiryYear { get; init; }
    
    public required string Currency { get; init; }
    
    public required int Amount { get; init; }
}
