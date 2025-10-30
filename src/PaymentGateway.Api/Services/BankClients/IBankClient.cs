using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services.BankClients;

public interface IBankClient
{
    public Task<BankAuthorizationResponse> AuthorizePaymentAsync(ProcessPaymentRequest paymentRequest);
}