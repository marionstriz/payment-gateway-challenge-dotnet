namespace PaymentGateway.Api.Tests.TestUtilities.Helpers;

public class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw exception;
    }
}