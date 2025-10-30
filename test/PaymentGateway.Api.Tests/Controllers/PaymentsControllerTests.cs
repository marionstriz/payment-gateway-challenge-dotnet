using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Persistence;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    private readonly HttpClient _client;
    
    private readonly Mock<IPaymentsRepository> _mockPaymentsRepository;

    public PaymentsControllerTests()
    {
        _mockPaymentsRepository = new Mock<IPaymentsRepository>();
        
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        _client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddSingleton(_mockPaymentsRepository.Object)))
            .CreateClient();
    }
    
    [Fact]
    public async Task GivenPaymentInRepository_WhenGetAsync_ThenRetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = CreatePaymentResponse();
        var paymentDao = CreatePaymentDao();
        _mockPaymentsRepository.Setup(r => r.GetAsync(payment.Id)).ReturnsAsync(paymentDao);

        // Act
        var response = await _client.GetAsync($"/api/Payments/{payment.Id}");
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
        // Act
        var response = await _client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GivenPaymentRequest_WhenPostAsync_ThenAddsPaymentSuccessfully()
    {
        // Arrange
        var postPaymentRequest = CreatePaymentRequest();
        var httpContent = CreateJsonHttpContent(postPaymentRequest);
        
        // Act
        var response = await _client.PostAsync("/api/Payments", httpContent);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private PaymentResponse CreatePaymentResponse()
    {
        return new PaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };
    }
    
    private PostPaymentRequest CreatePaymentRequest()
    {
        return new PostPaymentRequest
        {
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };
    }
    
    private PaymentDao CreatePaymentDao()
    {
        return new PaymentDao
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };
    }

    private HttpContent CreateJsonHttpContent(object content)
    {
        return new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
    }
}