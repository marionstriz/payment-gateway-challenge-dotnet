namespace PaymentGateway.Api.Settings;

public class BankClientSettings
{
    public required string BaseUrl { get; set; }
    
    public required string PaymentsPath { get; set; }
    
    public required int TimeoutSeconds { get; set; }
}