using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Persistence;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.BankClients;
using PaymentGateway.Api.Services.Repositories;

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
                    .AddSingleton(_mockBankClient.Object)))
            .CreateClient();
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
        Assert.Equal(paymentDao.Status, paymentResponse.Status);
        Assert.Equal(paymentDao.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(paymentDao.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(paymentDao.Amount, paymentResponse.Amount);
        Assert.Equal(paymentDao.CardNumberLastFour, paymentResponse.CardNumberLastFour);
        Assert.Equal(paymentDao.Currency, paymentResponse.Currency);
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
        var processPaymentRequest = CreateProcessPaymentRequest();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        var authorizationResponse = new BankAuthorizationResponse {Authorized = true};
        _mockBankClient.Setup(b => b.AuthorizePaymentAsync(processPaymentRequest))
            .ReturnsAsync(authorizationResponse);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse.Status);
    }
    
    [Fact]
    public async Task GivenRequestNotAuthorized_WhenPostAsync_ThenReturnsDeclinedPaymentResponse()
    {
        // Arrange
        var processPaymentRequest = CreateProcessPaymentRequest();
        var httpContent = CreateJsonHttpContent(processPaymentRequest);
        
        var authorizationResponse = new BankAuthorizationResponse {Authorized = false};
        _mockBankClient.Setup(b => b.AuthorizePaymentAsync(processPaymentRequest))
            .ReturnsAsync(authorizationResponse);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Declined, paymentResponse.Status);
    }
    
    private ProcessPaymentRequest CreateProcessPaymentRequest()
    {
        return new ProcessPaymentRequest
        {
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = "143487937937497",
            Currency = "GBP",
            Cvv = 123
        };
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
            Currency = "GBP"
        };
    }

    private HttpContent CreateJsonHttpContent(object content)
    {
        return new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
    }
}