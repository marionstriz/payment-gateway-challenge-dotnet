using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.Repositories;

public class InMemoryPaymentsRepository : IPaymentsRepository
{
    public List<PaymentResponse> Payments = new();
    
    public void Add(PaymentResponse payment)
    {
        Payments.Add(payment);
    }

    public PaymentResponse? Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}