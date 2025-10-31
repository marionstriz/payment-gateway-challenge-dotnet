namespace PaymentGateway.Api.Models;

public record PaymentAuthorizationResult
{
    public required bool Authorized { get; init; }
    public string? AuthorizationCode { get; init; }
}