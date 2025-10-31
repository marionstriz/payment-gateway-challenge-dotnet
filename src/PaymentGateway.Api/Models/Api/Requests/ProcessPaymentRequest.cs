using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Api.Requests;

public record ProcessPaymentRequest : IValidatableObject
{
    [Required]
    [RegularExpression(@"^\d+$", ErrorMessage = "Card number must be numeric.")]
    [StringLength(19, MinimumLength = 14, ErrorMessage = "Card number must be between 14-19 digits.")]
    [DefaultValue("11111111111111")]
    public required string CardNumber { get; init; }
    
    /// <summary>
    /// The combination of expiry month + year must be in the future.
    /// </summary>
    [Required]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    [DefaultValue("1")]
    public required int ExpiryMonth { get; init; }
    
    /// <summary>
    /// The combination of expiry month + year must be in the future.
    /// </summary>
    [Required]
    [DefaultValue("2030")]
    public required int ExpiryYear { get; init; }
    
    /// <summary>
    /// ISO currency code. Currently supported codes: [EUR, GBP, USD] 
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters.")]
    [DefaultValue(nameof(CurrencyCode.EUR))]
    public required string Currency { get; init; }
    
    /// <summary>
    /// Represents the amount in the minor currency unit.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive integer.")]
    [DefaultValue(10000)]
    public required int Amount { get; init; }
    
    [Required]
    [RegularExpression(@"^\d+$", ErrorMessage = "CVV must be numeric.")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3-4 digits.")]
    [DefaultValue("123")]
    public required string Cvv { get; init; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var now = DateTime.UtcNow;
        
        if (ExpiryYear < now.Year || (ExpiryYear == now.Year && ExpiryMonth < now.Month))
        {
            yield return new ValidationResult("Expiry date must be in the future.", 
                [nameof(ExpiryMonth), nameof(ExpiryYear)]
            );
        }

        if (Enum.TryParse(typeof(CurrencyCode), Currency, true, out _) is false)
        {
            yield return new ValidationResult("Currency code not supported.", [nameof(Currency)]);
        }
    }
}