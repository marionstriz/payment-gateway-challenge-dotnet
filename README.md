# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

# Design Notes

### Layer Separation

API requests/responses, persistence, and domain logic use distinct model classes to allow independent evolution.

Model translations currently occur directly in controllers or client classes for simplicity. 
If translations become more complex or repetitive, dedicated translator classes can be introduced to improve testability and reduce duplication.

### Currency Handling

`CurrencyCode` is an enum in the domain layer for type safety and validation.

API and persistence layers use strings to maintain flexibility and simplify client interactions and versioning.

### Bank Client Abstraction

`IBankClient` accepts `PaymentInfo` and returns `AuthorizationInfo`.

These objects do not directly map to bank-specific request/response formats, as banks may vary in field requirements 
(e.g., expiry date, currency format). Each bank client handles its own mapping.

### Versioning

API versioning ensures that future breaking changes do not directly impact clients.

## Assumptions

- `Amount` fits within an integer. For currencies with small units (e.g., IDR) or very large transactions, consider using long. 
- Multiple bank client implementations are expected; if only one is ever used, mapping can be simplified.
- Only failed payment requests are logged; successful requests do not require info-level logging.
- No authentication/authorization is required at this stage.