using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Persistence;

namespace PaymentGateway.Api.Services.Repositories;

public class InMemoryPaymentsRepository : IPaymentsRepository
{
    private readonly Dictionary<Guid, PaymentDao> _payments = new();
    
    public Task<Payment> AddAsync(Payment payment)
    {
        var paymentDao = Map(payment);
        _payments.Add(payment.Id, paymentDao);
        return Task.FromResult(payment);
    }

    public Task<Payment?> GetAsync(Guid id)
    {
        var paymentDao = _payments.GetValueOrDefault(id);
        return Task.FromResult(Map(paymentDao));
    }

    private Payment? Map(PaymentDao? paymentDao)
    {
        if (paymentDao is null)
        {
            return null;
        }
        return new Payment
        {
            Id = paymentDao.Id,
            Status = paymentDao.Status,
            Currency = paymentDao.Currency,
            CardNumberLastFour = paymentDao.CardNumberLastFour,
            Amount = paymentDao.Amount,
            ExpiryMonth = paymentDao.ExpiryMonth,
            ExpiryYear = paymentDao.ExpiryYear
        };
    }
    
    private static PaymentDao Map(Payment payment)
    {
        var paymentDao = new PaymentDao
        {
            Id = payment.Id,
            Status = payment.Status,
            Currency = payment.Currency,
            CardNumberLastFour = payment.CardNumberLastFour,
            Amount = payment.Amount,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear
        };
        return paymentDao;
    }
}