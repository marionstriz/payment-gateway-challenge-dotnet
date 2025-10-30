namespace PaymentGateway.Api.Models.Persistence;

public record PaymentDao
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required PaymentStatus Status { get; init; }
    public required string CardNumberLastFour { get; init; }
    public required int ExpiryMonth { get; init; }
    public required int ExpiryYear { get; init; }
    public required string Currency { get; init; }
    public required int Amount { get; init; }
}