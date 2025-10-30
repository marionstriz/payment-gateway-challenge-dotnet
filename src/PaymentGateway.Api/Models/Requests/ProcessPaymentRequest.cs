using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public record ProcessPaymentRequest : IValidatableObject
{
    [Required]
    [RegularExpression(@"^\d+$", ErrorMessage = "Card number must be numeric.")]
    [StringLength(19, MinimumLength = 14, ErrorMessage = "Card number must be between 14-19 digits.")]
    public required string CardNumber { get; init; }
    
    [Required]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public required int ExpiryMonth { get; init; }
    
    [Required]
    public required int ExpiryYear { get; init; }
    
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters.")]
    public required string Currency { get; init; }
    
    [Required]
    public required int Amount { get; init; }
    
    [Required]
    public required string Cvv { get; init; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var now = DateTime.UtcNow;
        
        if (ExpiryYear < now.Year || (ExpiryYear == now.Year && ExpiryMonth < now.Month))
        {
            yield return new ValidationResult("Expiry date must be in the future", 
                [nameof(ExpiryMonth), nameof(ExpiryYear)]
            );
        }
    }
}