namespace PaymentGateway.Api.Models.Api.Responses;

public class ErrorResponse
{
    public required string Message { get; init; }
    public required int Code { get; init; }
}