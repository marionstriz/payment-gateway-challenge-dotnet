using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Api.Requests;

namespace PaymentGateway.Api.Tests.TestUtilities.Builders;

public class ProcessPaymentRequestBuilder
{
    private string _cardNumber = "4111111111111111";
    private int _expiryMonth = DateTime.UtcNow.Month;
    private int _expiryYear = DateTime.UtcNow.Year;
    private string _currency = nameof(CurrencyCode.USD);
    private int _amount = 100;
    private string _cvv = "123";

    public ProcessPaymentRequestBuilder WithCardNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }

    public ProcessPaymentRequestBuilder WithExpiryMonth(int month)
    {
        _expiryMonth = month;
        return this;
    }

    public ProcessPaymentRequestBuilder WithExpiryYear(int year)
    {
        _expiryYear = year;
        return this;
    }

    public ProcessPaymentRequestBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public ProcessPaymentRequestBuilder WithAmount(int amount)
    {
        _amount = amount;
        return this;
    }

    public ProcessPaymentRequestBuilder WithCvv(string cvv)
    {
        _cvv = cvv;
        return this;
    }

    public ProcessPaymentRequest Build()
    {
        return new ProcessPaymentRequest
        {
            CardNumber = _cardNumber,
            ExpiryMonth = _expiryMonth,
            ExpiryYear = _expiryYear,
            Currency = _currency,
            Amount = _amount,
            Cvv = _cvv
        };
    }
}
