namespace PaymentGateway.Api.Models.Persistence;

public record PaymentDao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required PaymentStatus Status { get; set; }
    public required string CardNumberLastFour { get; set; }
    public required int ExpiryMonth { get; set; }
    public required int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    public required int Amount { get; set; }
}