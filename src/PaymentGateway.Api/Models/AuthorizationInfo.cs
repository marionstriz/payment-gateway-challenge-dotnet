namespace PaymentGateway.Api.Models;

public record AuthorizationInfo
{
    public required bool Authorized { get; set; }
    public string? AuthorizationCode { get; set; }
}