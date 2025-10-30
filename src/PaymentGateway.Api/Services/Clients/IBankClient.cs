using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services.Clients;

public interface IBankClient
{
    public Task<AuthorizationInfo> AuthorizePaymentAsync(PaymentInfo info);
}