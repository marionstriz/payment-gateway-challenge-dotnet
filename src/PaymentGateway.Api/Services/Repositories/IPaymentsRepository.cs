using PaymentGateway.Api.Models.Persistence;

namespace PaymentGateway.Api.Services.Repositories;

public interface IPaymentsRepository
{
    public Task AddAsync(PaymentDao payment);
    public Task<PaymentDao?> GetAsync(Guid id);
}