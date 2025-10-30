using Microsoft.Extensions.Options;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Settings;

namespace PaymentGateway.Api.Services.Clients;

public class MountebankBankClient(
    IHttpClientFactory httpClientFactory, 
    IOptions<BankClientSettings> clientOptions) : IBankClient
{
    public const string HttpClientName = "Mountebank";
    
    public async Task<AuthorizationInfo> AuthorizePaymentAsync(PaymentInfo info, CancellationToken cancellationToken = new())
    {
        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);

            var paymentAuthorizationRequest = CreateRequest(info);

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
                $"Payment authorization HTTP request failed: {e.HttpRequestError.ToString()} {e.Message}.", info);
        }
        catch (TaskCanceledException e)
        {
            throw new PaymentAuthorizationFailedException(
                $"Payment authorization request canceled: {e.Message}.", info);
        }
    }

    private AuthorizationInfo CreateAuthorizationInfo(MountebankPaymentAuthorizationResponse authorizationResponse)
    {
        return new AuthorizationInfo
        {
            Authorized = authorizationResponse.Authorized,
            AuthorizationCode = authorizationResponse.AuthorizationCode
        };
    }

    private MountebankPaymentAuthorizationRequest CreateRequest(PaymentInfo info)
    {
        return new MountebankPaymentAuthorizationRequest
        {
            Amount = info.Amount, 
            CardNumber = info.CardNumber, 
            Currency = info.Currency.ToString(), 
            Cvv = info.Cvv,
            ExpiryDate = $"{info.ExpiryMonth:D2}/{info.ExpiryYear}"
        };
    }
}