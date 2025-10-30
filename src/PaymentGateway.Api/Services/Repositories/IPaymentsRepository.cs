using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Repositories;

public interface IPaymentsRepository
{
    public void Add(PaymentResponse payment);
    public PaymentResponse? Get(Guid id);
}