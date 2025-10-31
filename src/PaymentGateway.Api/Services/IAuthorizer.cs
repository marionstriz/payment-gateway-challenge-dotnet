using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public interface IAuthorizer
{
    public Task<PaymentAuthorizationResult> AuthorizePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = new());
}