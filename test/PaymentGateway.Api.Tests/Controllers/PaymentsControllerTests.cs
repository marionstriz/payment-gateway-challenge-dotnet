using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Persistence;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Clients;
using PaymentGateway.Api.Services.Repositories;
using PaymentGateway.Api.Tests.TestUtilities.Builders;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly HttpClient _client;
    
    private readonly Mock<IPaymentsRepository> _mockPaymentsRepository;
    private readonly Mock<IBankClient> _mockBankClient;

    public PaymentsControllerTests()
    {
        _mockPaymentsRepository = new Mock<IPaymentsRepository>();
        _mockBankClient = new Mock<IBankClient>();
        
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        _client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddSingleton(_mockPaymentsRepository.Object)
                    .AddSingleton(_mockBankClient.Object)
                    .Configure<ApiBehaviorOptions>(options =>
                    {
                        options.SuppressModelStateInvalidFilter = true;
                    })))
            .CreateClient();
        
        var authorizationResponse = new AuthorizationInfo{Authorized = true};
        _mockBankClient.Setup(b => b.AuthorizePaymentAsync(It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorizationResponse);
    }
    
    [Fact]
    public async Task GivenPaymentInRepository_WhenGetAsync_ThenRetrievesAPaymentSuccessfully()
    {
        // Arrange
        var paymentDao = CreatePaymentDao();
        _mockPaymentsRepository.Setup(r => r.GetAsync(paymentDao.Id)).ReturnsAsync(paymentDao);

        // Act
        var response = await _client.GetAsync($"/api/Payments/{paymentDao.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }
    
    [Fact]
    public async Task GivenPaymentInRepository_WhenGetAsync_ThenReturnsPaymentDetails()
    {
        // Arrange
        var paymentDao = CreatePaymentDao();
        _mockPaymentsRepository.Setup(r => r.GetAsync(paymentDao.Id)).ReturnsAsync(paymentDao);

        // Act
        var response = await _client.GetAsync($"/api/Payments/{paymentDao.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(paymentDao.Id, paymentResponse.Id);
        Assert.Equal(paymentDao.Status.ToString(), paymentResponse.Status);
        Assert.Equal(paymentDao.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(paymentDao.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(paymentDao.Amount, paymentResponse.Amount);
        Assert.Equal(paymentDao.CardNumberLastFour, paymentResponse.CardNumberLastFour);
        Assert.Equal(paymentDao.Currency.ToString(), paymentResponse.Currency);
    }

    [Fact]
    public async Task GivenPaymentNotInRepository_WhenGetAsync_ThenReturns404IfPaymentNotFound()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _mockPaymentsRepository.Setup(r => r.GetAsync(guid)).ReturnsAsync(null as PaymentDao);
        
        // Act
        var response = await _client.GetAsync($"/api/Payments/{guid}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GivenRequestAuthorized_WhenPostAsync_ThenReturnsApprovedPaymentResponse()
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder().Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        var authorizationResponse = new AuthorizationInfo {Authorized = true};
        _mockBankClient.Setup(b => b.AuthorizePaymentAsync(It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorizationResponse);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(nameof(PaymentStatus.Authorized), paymentResponse.Status);
    }
    
    [Fact]
    public async Task GivenValidRequest_WhenPostAsync_ThenCallsBankClientWithMappedData()
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder().Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        await _client.PostAsync("/api/Payments", httpContent);
        
        // Assert
        _mockBankClient.Verify(b => b.AuthorizePaymentAsync(It.Is<PaymentInfo>(r => 
            r.CardNumber.Equals(processPaymentRequest.CardNumber) 
            && r.Amount.Equals(processPaymentRequest.Amount) 
            && r.ExpiryMonth.Equals(processPaymentRequest.ExpiryMonth) 
            && r.ExpiryYear.Equals(processPaymentRequest.ExpiryYear) 
            && r.Cvv.Equals(processPaymentRequest.Cvv)), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockBankClient.VerifyNoOtherCalls();
    }
    
    [Theory]
    [InlineData("EUR", CurrencyCode.EUR)]
    [InlineData("Usd", CurrencyCode.USD)]
    [InlineData("gbp", CurrencyCode.GBP)]
    public async Task GivenValidCurrency_WhenPostAsync_ThenCallsBankClientWithExpectedCurrency(
        string currencyCode, CurrencyCode expectedCurrency)
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithCurrency(currencyCode)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        await _client.PostAsync("/api/Payments", httpContent);
        
        // Assert
        _mockBankClient.Verify(b => b.AuthorizePaymentAsync(It.Is<PaymentInfo>(r => 
                r.Currency.Equals(expectedCurrency)), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockBankClient.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task GivenRequestNotAuthorized_WhenPostAsync_ThenReturnsDeclinedPaymentResponse()
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder().Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        var authorizationResponse = new AuthorizationInfo {Authorized = false};
        _mockBankClient.Setup(b => b.AuthorizePaymentAsync(It.IsAny<PaymentInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorizationResponse);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(nameof(PaymentStatus.Declined), paymentResponse.Status);
    }
    
    [Theory]
    [InlineData("1234567898765")]
    [InlineData("98765432123456789876")]
    [InlineData("9876543212345678f")]
    public async Task GivenInvalidCardNumber_WhenPostAsync_ThenReturns400RejectedPaymentResponse(string cardNumber)
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithCardNumber(cardNumber)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(13)]
    public async Task GivenInvalidExpiryMonth_WhenPostAsync_ThenReturns400RejectedPaymentResponse(int expiryMonth)
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithExpiryMonth(expiryMonth)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Theory]
    [InlineData("EURO")]
    [InlineData("BGN")]
    [InlineData("eur ")]
    public async Task GivenInvalidCurrency_WhenPostAsync_ThenReturns400RejectedPaymentResponse(string currencyCode)
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithCurrency(currencyCode)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task GivenInvalidAmount_WhenPostAsync_ThenReturns400RejectedPaymentResponse(int amount)
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithAmount(amount)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Theory]
    [InlineData("12")]
    [InlineData("12555")]
    [InlineData("abc")]
    public async Task GivenInvalidCvv_WhenPostAsync_ThenReturns400RejectedPaymentResponse(string cvv)
    {
        // Arrange
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithCvv(cvv)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Fact]
    public async Task GivenExpiryDateOneMonthAgo_WhenPostAsync_ThenReturns400RejectedPaymentResponse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var future = now.Subtract(TimeSpan.FromDays(32));
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithExpiryYear(future.Year)
            .WithExpiryMonth(future.Month)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Fact]
    public async Task GivenExpiryDateOneYearAgo_WhenPostAsync_ThenReturns400RejectedPaymentResponse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var future = now.Subtract(TimeSpan.FromDays(365));
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithExpiryYear(future.Year)
            .WithExpiryMonth(future.Month)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(nameof(PaymentStatus.Rejected), paymentResponse.Status);
    }
    
    [Fact]
    public async Task GivenExpiryDateThisMonth_WhenPostAsync_ThenReturnsAuthorizedPaymentResponse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var processPaymentRequest = new ProcessPaymentRequestBuilder()
            .WithExpiryYear(now.Year)
            .WithExpiryMonth(now.Month)
            .Build();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(nameof(PaymentStatus.Authorized), paymentResponse.Status);
    }
    
    private PaymentDao CreatePaymentDao()
    {
        return new PaymentDao
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = "9876",
            Currency = CurrencyCode.GBP
        };
    }

    private HttpContent CreateJsonHttpContent(object content)
    {
        return new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
    }
}