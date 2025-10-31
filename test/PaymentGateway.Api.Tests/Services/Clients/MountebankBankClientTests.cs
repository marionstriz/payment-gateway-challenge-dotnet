using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Options;

using Moq;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.External;
using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Settings;
using PaymentGateway.Api.Tests.TestUtilities.Helpers;

namespace PaymentGateway.Api.Tests.Services.Clients;

public class MountebankBankClientTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IOptions<BankClientSettings>> _optionsMock;

    private readonly MountebankBankClient _bankClient;

    private readonly BankClientSettings _settings = new()
    {
        PaymentsPath = "payments",
        BaseUrl = "https://random.com/",
        TimeoutSeconds = 10
    };

    public MountebankBankClientTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _optionsMock = new Mock<IOptions<BankClientSettings>>();
        _optionsMock.Setup(o => o.Value).Returns(_settings);
        
        _bankClient = new MountebankBankClient(_httpClientFactoryMock.Object, _optionsMock.Object);
    }

    [Fact]
    public async Task GivenAuthorizationResponse_WhenAuthorizePaymentAsync_ThenReturnsAuthorizationInfo()
    {
        // Arrange
        var authorizationCode = "AUTH123";
        var expectedResponse = new MountebankPaymentAuthorizationResponse
        {
            Authorized = true,
            AuthorizationCode = authorizationCode
        };

        var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedResponse)
        });

        SetupHttpClientWithFactory(handler);
        
        var paymentInfo = CreatePaymentInfo();

        // Act
        var result = await _bankClient.AuthorizePaymentAsync(paymentInfo);

        // Assert
        Assert.True(result.Authorized);
        Assert.Equal(authorizationCode, result.AuthorizationCode);
    }

    [Fact]
    public async Task GivenNonSuccessStatusCodeResponse_WhenAuthorizePaymentAsync_ThenThrowsAuthorizationFailedException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Invalid")
        });

        SetupHttpClientWithFactory(handler);
        
        var paymentInfo = CreatePaymentInfo();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<PaymentAuthorizationFailedException>(() =>
            _bankClient.AuthorizePaymentAsync(paymentInfo));
        
        Assert.Contains(((int)HttpStatusCode.BadRequest).ToString(), ex.Message);
    }

    [Fact]
    public async Task GivenHttpRequestException_WhenAuthorizePaymentAsync_ThenThrowsAuthorizationFailedException()
    {
        // Arrange
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection failed"));

        SetupHttpClientWithFactory(handler);
        
        var paymentInfo = CreatePaymentInfo();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<PaymentAuthorizationFailedException>(() =>
            _bankClient.AuthorizePaymentAsync(paymentInfo));
        
        Assert.Contains("Montebank payment authorization request failed", ex.Message);
    }

    private PaymentRequest CreatePaymentInfo()
    {
        return new PaymentRequest
        {
            Amount = 100,
            CardNumber = "4111111111111111",
            Currency = CurrencyCode.USD,
            Cvv = "123",
            ExpiryMonth = 12,
            ExpiryYear = 2030
        };
    }

    private void SetupHttpClientWithFactory(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClientFactoryMock.Setup(f => f.CreateClient(MountebankBankClient.HttpClientName))
            .Returns(httpClient);
    }
}