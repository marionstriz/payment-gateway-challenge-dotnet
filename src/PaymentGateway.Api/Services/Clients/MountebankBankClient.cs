using Microsoft.Extensions.Options;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.External;
using PaymentGateway.Api.Settings;

namespace PaymentGateway.Api.Services.Clients;

public class MountebankBankClient(
    IHttpClientFactory httpClientFactory, 
    IOptions<BankClientSettings> clientOptions) : IAuthorizer
{
    public const string HttpClientName = "Mountebank";
    
    public async Task<PaymentAuthorizationResult> AuthorizePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = new())
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            var paymentAuthorizationRequest = CreateRequest(request);

            var response = await client.PostAsJsonAsync(clientOptions.Value.PaymentsPath, 
                paymentAuthorizationRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var authorizationResponse = await response.Content
                .ReadFromJsonAsync<MountebankPaymentAuthorizationResponse>(cancellationToken);
            
            return CreateAuthorizationInfo(authorizationResponse!);
        }
        catch (HttpRequestException e)
        {
            throw new PaymentAuthorizationFailedException(
                $"Montebank payment authorization request failed: {e.Message}.");
        }
        catch (TaskCanceledException e)
        {
            throw new PaymentAuthorizationFailedException(
                $"Montebank payment authorization request canceled: {e.Message}.");
        }
    }

    private PaymentAuthorizationResult CreateAuthorizationInfo(MountebankPaymentAuthorizationResponse authorizationResponse)
    {
        return new PaymentAuthorizationResult
        {
            Authorized = authorizationResponse.Authorized,
            AuthorizationCode = authorizationResponse.AuthorizationCode
        };
    }

    private MountebankPaymentAuthorizationRequest CreateRequest(PaymentRequest request)
    {
        return new MountebankPaymentAuthorizationRequest
        {
            Amount = request.Amount, 
            CardNumber = request.CardNumber, 
            Currency = request.Currency.ToString(), 
            Cvv = request.Cvv,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}"
        };
    }
}