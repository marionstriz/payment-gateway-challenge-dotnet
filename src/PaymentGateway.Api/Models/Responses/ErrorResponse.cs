using System.Net;

namespace PaymentGateway.Api.Models.Responses;

public class ErrorResponse
{
    public required string Message { get; set; }
    public required HttpStatusCode Code { get; set; }
}