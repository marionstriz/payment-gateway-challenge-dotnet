using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public interface IAuthorizer
{
    public Task<AuthorizationInfo> AuthorizePaymentAsync(PaymentInfo info, CancellationToken cancellationToken = new());
}