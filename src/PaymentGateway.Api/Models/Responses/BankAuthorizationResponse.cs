namespace PaymentGateway.Api.Models.Responses;

public record BankAuthorizationResponse
{
    public bool Authorized { get; set; }
    public string? AuthorizationCode { get; set; }
}