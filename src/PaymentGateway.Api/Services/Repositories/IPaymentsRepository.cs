using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services.Repositories;

public interface IPaymentsRepository
{
    public Task<Payment> AddAsync(Payment payment);
    public Task<Payment?> GetAsync(Guid id);
}