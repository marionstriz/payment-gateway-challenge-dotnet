using PaymentGateway.Api.Models.Persistence;

namespace PaymentGateway.Api.Services.Repositories;

public class InMemoryPaymentsRepository : IPaymentsRepository
{
    private readonly Dictionary<Guid, PaymentDao> _payments = new();
    
    public Task<PaymentDao> AddAsync(PaymentDao payment)
    {
        _payments.Add(payment.Id, payment);
        return Task.FromResult(payment);
    }

    public Task<PaymentDao?> GetAsync(Guid id)
    {
        return Task.FromResult(_payments.GetValueOrDefault(id));
    }
}