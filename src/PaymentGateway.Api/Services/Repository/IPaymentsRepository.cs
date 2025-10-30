using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Repository;

public interface IPaymentsRepository
{
    public void Add(PaymentResponse payment);
    public PaymentResponse? Get(Guid id);
}