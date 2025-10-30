namespace PaymentGateway.Api.Models.Responses;

public record PaymentResponse
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required PaymentStatus Status { get; set; }
    public required string CardNumberLastFour { get; set; }
    public required int ExpiryMonth { get; set; }
    public required int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    public required int Amount { get; set; }
}
