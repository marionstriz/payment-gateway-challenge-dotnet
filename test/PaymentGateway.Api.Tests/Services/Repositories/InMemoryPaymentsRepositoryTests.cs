using PaymentGateway.Api.Models.Persistence;
using PaymentGateway.Api.Services.Repositories;

namespace PaymentGateway.Api.Tests.Services.Repositories;

public class InMemoryPaymentsRepositoryTests
{
    private readonly Random _random = new();
    
    private readonly InMemoryPaymentsRepository _repository = new();

    [Fact]
    public async Task GivenPaymentAdded_WhenGetAsync_ThenReturnsPayment()
    {
        // Arrange
        var initialPayment = CreatePaymentDao();
        await _repository.AddAsync(initialPayment);
        
        // Act
        var actualPayment = await _repository.GetAsync(initialPayment.Id);
        
        // Assert
        Assert.NotNull(actualPayment);
        Assert.Equal(initialPayment, actualPayment);
    }
    
    [Fact]
    public async Task GivenPaymentNotAdded_WhenGetAsync_ThenReturnsNull()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var payment = await _repository.GetAsync(guid);

        // Assert
        Assert.Null(payment);
    }
    
    [Fact]
    public async Task GivenPaymentWithIdExists_WhenAddAsync_ThenThrowsArgumentExceptionWithId()
    {
        // Arrange
        var initialPayment = CreatePaymentDao();
        await _repository.AddAsync(initialPayment);
        
        // Act
        var addAction = async() => await _repository.AddAsync(initialPayment);
        
        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(addAction);
        Assert.Contains(initialPayment.Id.ToString(), exception.Message);
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
}