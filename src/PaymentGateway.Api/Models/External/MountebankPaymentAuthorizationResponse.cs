using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.External;

public record MountebankPaymentAuthorizationResponse
{
    [JsonPropertyName("authorized")]
    public required bool Authorized { get; set; }
    
    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; set; }
}