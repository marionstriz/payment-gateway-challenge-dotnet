using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Api.Responses;

public record PaymentResponse
{
    [Required]
    public required Guid Id { get; init; }
    
    /// <summary>
    /// One of the following values: [Authorized, Declined, Rejected]
    /// </summary>
    [Required]
    public required string Status { get; init; }
    
    [Required]
    public required string CardNumberLastFour { get; init; }
    
    [Required]
    public required int ExpiryMonth { get; init; }
    
    [Required]
    public required int ExpiryYear { get; init; }
    
    /// <summary>
    /// ISO currency code. Currently supported codes: [EUR, GBP, USD] 
    /// </summary>
    [Required]
    public required string Currency { get; init; }
    
    /// <summary>
    /// Represents the amount in the minor currency unit.
    /// </summary>
    [Required]
    public required int Amount { get; init; }
}
