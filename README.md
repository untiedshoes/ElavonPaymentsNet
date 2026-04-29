# ElavonPaymentsNet

[![CI](https://img.shields.io/github/actions/workflow/status/untiedshoes/ElavonPaymentsNet/dotnet.yml?label=CI)](https://github.com/untiedshoes/ElavonPaymentsNet/actions)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![License](https://img.shields.io/github/license/untiedshoes/ElavonPaymentsNet)](LICENSE)

> A production-grade .NET 10 SDK for the [Elavon / Opayo Payments PI REST API](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference).

`ElavonPaymentsNet` wraps the Opayo REST API behind a clean, predictable client surface. It handles authentication, serialisation, HTTP error mapping, and 3D Secure flows internally, leaving the public API shallow and hard to misuse.

The design is intentionally Stripe-style: a single client entry point with named service groups, typed request and response models, and no infrastructure concerns leaking into the calling code.

---

## What this project demonstrates

- Designing a production-grade .NET SDK over a third-party payment REST API
- Keeping the public surface shallow and predictable (`client.Transactions.CreateTransactionAsync(...)`)
- Centralising all HTTP concerns in a single internal `ElavonApiClient` (auth, serialisation, error mapping)
- Typed exception hierarchy so consumers can handle payment errors, auth failures, and API errors distinctly
- Service interfaces (`IElavonTransactionService` etc.) enabling clean mocking in consumer test suites
- Clean DI registration via `IHttpClientFactory` for production use
- Safe-by-default transient fault handling in the HTTP infrastructure layer only
- Unit testing of mappers and client guard clauses without a live API

---

## Features

- Full coverage of the Opayo PI REST API: payments, post-payment operations, 3D Secure, tokenisation, Apple Pay, card identifiers, and instructions
- Typed request and response models -- no raw JSON or `dynamic` in the public API
- `Basic` authentication for all standard API calls; `Bearer` token support for MSK/drop-in flows
- All HTTP errors converted to typed `ElavonApiException` -- raw HTTP responses never surface to the caller
- `ElavonAuthenticationException` on 401 so authentication failures are always unambiguous
- Public interfaces on all service groups for consumer-side mocking
- First-class `CancellationToken` support on all async operations
- DI-friendly: register once via `services.AddElavonPayments(...)` using `IHttpClientFactory`
- Polly-based retry policy in a central HTTP delegating handler (`ElavonResilienceHandler`)
- Automatic retries are GET-only; POST operations are never retried to avoid duplicate financial side effects
- Direct instantiation supported for console or scripting use cases
- Target framework: `net10.0`

---

## Full Flow Diagram

```
+-----------------------------------------------------------------------------------+
|                              ElavonPaymentsClient                                 |
|                                                                                   |
|  .Transactions         .PostPayments       .Instructions  .ThreeDs               |
|  .Tokens               .Wallets            .CardIdentifiers                       |
+------------------------------------+----------------------------------------------+
                                     |
                          (all calls route through)
                                     |
                         +-----------v-----------+
                         |    ElavonApiClient     |  <- internal
                         |  (HttpClient wrapper)  |
                         |                        |
                         |  Auth: Basic or Bearer |
                         |  Serialise (camelCase) |
                         |  Map errors -> typed ex|
                         +-----------+-----------+
                                     |  HTTPS
                          +----------v----------+
                          |   Opayo REST API    |
                          |   sandbox / live    |
                          +---------------------+

---------------------------------------------------------------------------
STANDARD PAYMENT FLOW (Basic auth throughout)
---------------------------------------------------------------------------

 Client                   SDK                       Opayo API
   |                       |                             |
   | CreateTransaction     |                             |
   |  Async(Payment)       |                             |
   |---------------------->|  POST /transactions         |
   |                       |  Authorization: Basic ...   |
   |                       |---------------------------->|
   |                       |                             |
   |                       |  200 OK { transactionId,   |
   |                       |           status: "Ok" }   |
   |                       |<----------------------------|
   | PaymentResponse       |                             |
   |<----------------------|                             |


---------------------------------------------------------------------------
3D SECURE FLOW
---------------------------------------------------------------------------

 Client                   SDK                       Opayo API
   |                       |                             |
   | CreateTransactionAsync|                             |
   |---------------------->|  POST /transactions         |
   |                       |---------------------------->|
   |                       |  202 { status: "3DAuth",   |
   |                       |        acsUrl, cReq }      |
   |                       |<----------------------------|
   | PaymentResponse       |                             |
   | (.Status="3DAuth")    |                             |
   |<----------------------|                             |
   |                       |                             |
   |  [redirect cardholder to acsUrl with cReq]         |
   |  [ACS authenticates, posts cRes to your callback]  |
   |                       |                             |
   | Initialise3DsAsync    |  POST /transactions/{id}   |
   |---------------------->|       /3d-secure            |
   |                       |---------------------------->|
   |                       |<----------------------------|
   | Initialise3DsResponse |                             |
   |<----------------------|                             |
   |                       |                             |
   | Complete3DsAsync(cRes)|  POST /transactions/{id}   |
   |---------------------->|       /3d-secure/complete   |
   |                       |---------------------------->|
   |                       |  200 { status: "Ok" }      |
   |                       |<----------------------------|
   | Complete3DsResponse   |                             |
   |<----------------------|                             |


---------------------------------------------------------------------------
DROP-IN / CARD IDENTIFIER FLOW (MSK + Bearer auth)
---------------------------------------------------------------------------

 Client                   SDK                       Opayo API
   |                       |                             |
   | CreateMerchantSession |  POST /merchant-session-    |
   |  KeyAsync             |       keys                  |
   |---------------------->|  Authorization: Basic ...   |
   |                       |---------------------------->|
   |                       |  200 { merchantSessionKey } |
   |                       |<----------------------------|
   | MerchantSessionResp.  |                             |
   |<----------------------|                             |
   |                       |                             |
   | CreateCardIdentifier  |  POST /card-identifiers     |
   |  Async(msk, request)  |  Authorization: Bearer {msk}|
   |---------------------->|---------------------------->|
   |                       |  200 { cardIdentifier,     |
   |                       |        expiry, cardType }  |
   |                       |<----------------------------|
   | CreateCardIdentifier  |                             |
   |  Response             |                             |
   |<----------------------|                             |
   |                       |                             |
   | CreateTransactionAsync|  POST /transactions         |
   |  (Token=cardIdent.)   |  Authorization: Basic ...   |
   |---------------------->|---------------------------->|
   |                       |<----------------------------|
   | PaymentResponse       |                             |
   |<----------------------|                             |


---------------------------------------------------------------------------
POST-PAYMENT & INSTRUCTIONS
---------------------------------------------------------------------------

  After Authorise or Deferred:

  CaptureTransactionAsync   ->  POST /transactions/{id}/capture
  RefundTransactionAsync    ->  POST /transactions/{id}/refund
  VoidTransactionAsync      ->  POST /transactions/{id}/void

  Lifecycle instructions (Void, Abort, Release, Cancel):

  CreateInstructionAsync    ->  POST /transactions/{id}/instructions
```

---

## Quick Example

```csharp
var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
{
    IntegrationKey      = "your-integration-key",
    IntegrationPassword = "your-integration-password",
    Environment         = ElavonEnvironment.Sandbox
});

var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = "ORDER-001",
    Amount          = 1999,   // pence -- GBP 19.99
    Currency        = "GBP",
    Description     = "Widget purchase",
    PaymentMethod   = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber     = "4929000000006",
            ExpiryDate     = "1229",
            SecurityCode   = "123",
            CardholderName = "Craig Richards"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1   = "88 Test Street",
        City       = "London",
        PostalCode = "EC1A 1BB",
        Country    = "GB"
    }
});

Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Transaction: {result.TransactionId}");
```

---

## Authentication

The SDK uses HTTP `Basic` authentication for all standard API calls. Your `IntegrationKey` and `IntegrationPassword` are Base64-encoded and sent as the `Authorization` header on every request.

Merchant session key and card identifier flows use `Bearer` token authentication. Pass the session key into `CreateCardIdentifierAsync` -- the SDK switches the header automatically.

Credentials are held in `ElavonPaymentsClientOptions` and never exposed on service methods or response models.

---

## Dependency Injection

```csharp
services.AddElavonPayments(options =>
{
    options.IntegrationKey      = builder.Configuration["Elavon:IntegrationKey"]!;
    options.IntegrationPassword = builder.Configuration["Elavon:IntegrationPassword"]!;
    options.Environment         = ElavonEnvironment.Live;
    options.Timeout             = TimeSpan.FromSeconds(30); // optional, 30s default
});
```

Inject `ElavonPaymentsClient` directly, or inject individual service interfaces for easier mocking:

```csharp
public class OrderService(IElavonTransactionService transactions)
{
    public async Task<string> ChargeAsync(int amountPence, string vendorTxCode)
    {
        var result = await transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode    = vendorTxCode,
            Amount          = amountPence,
            Currency        = "GBP",
            PaymentMethod   = new PaymentMethod { Token = "stored-token" }
        });

        return result.TransactionId ?? throw new Exception("No transaction ID returned.");
    }
}
```

### Mocking in tests

All service properties on `ElavonPaymentsClient` are typed as interfaces:

```csharp
var mockTransactions = new Mock<IElavonTransactionService>();
mockTransactions
    .Setup(x => x.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default))
    .ReturnsAsync(new PaymentResponse { Status = "Ok", TransactionId = "abc-123" });
```

---

## Error Handling

All API errors are mapped to typed exceptions -- the raw `HttpResponseMessage` never reaches the caller.

| Exception | When thrown |
|---|---|
| `ElavonAuthenticationException` | 401 Unauthorised -- invalid or missing credentials |
| `ElavonApiException` | Any other non-success HTTP status (4xx, 5xx) |

Both expose `HttpStatusCode` (as `int`), `RawResponse` (the original API body), and optionally `ErrorCode`.

```csharp
try
{
    var result = await client.Transactions.CreateTransactionAsync(request);
}
catch (ElavonAuthenticationException)
{
    // Invalid integration key or password
}
catch (ElavonApiException ex) when (ex.HttpStatusCode == 422)
{
    // Validation error -- inspect ex.RawResponse for detail
}
catch (ElavonApiException ex)
{
    Console.WriteLine($"{ex.HttpStatusCode}: {ex.ErrorCode}");
}
```

---

## Resilience and Retry Safety

Retry behavior is implemented in one place only: the HTTP infrastructure layer (`ElavonResilienceHandler`).
It is not implemented in services, models, or public SDK methods.

### Current retry policy

- Retry scope: GET requests only
- Retries up to 3 attempts after initial failure (`MaxRetryAttempts`, default `3`)
- Backoff: exponential with jitter
- Retries on:
    - `HttpRequestException`
    - `TaskCanceledException` (timeout)
    - `5xx` HTTP responses
- No retries on `4xx` HTTP responses

### Why POST is never retried

This SDK performs financial operations (payment, capture, refund, void, instruction, tokenisation) via POST endpoints.
Automatically retrying those calls can create duplicate side effects if the first request reached Opayo but the response was lost.

### Why no idempotency keys

The Opayo PI REST API does not currently provide an `Idempotency-Key` request-header contract comparable to Stripe.
Without a provider-enforced idempotency key guarantee, retrying POST is not considered safe.

Decision rule used by the SDK:

> Prefer not retrying over risking duplicate financial operations.

---

## SDK Service Reference

All methods are `async` and accept a `CancellationToken`. Each service group has a corresponding interface.

### `client.Transactions` -- `IElavonTransactionService`

| Method | Description |
|---|---|
| `CreateTransactionAsync(request)` | Create a transaction -- `TransactionType` determines behaviour: `Payment`, `Authorise`, `Deferred`, or `Repeat` |

Returns `PaymentResponse` -- `TransactionId`, `Status`, `StatusCode`, `StatusDetail`, `ThreeDSecure`.

---

### `client.PostPayments` -- `IElavonPostPaymentService`

| Method | Description |
|---|---|
| `CaptureTransactionAsync(transactionId, request)` | Capture a deferred or authorised payment |
| `RefundTransactionAsync(transactionId, request)` | Refund a settled payment, fully or partially |
| `VoidTransactionAsync(transactionId)` | Void a payment that has not yet settled |

Returns `PostPaymentResponse` -- `TransactionId`, `Status`.

---

### `client.Instructions` -- `IElavonInstructionsService`

| Method | Description |
|---|---|
| `CreateInstructionAsync(transactionId, request)` | Post a lifecycle instruction to a transaction |

`InstructionType` values: `Void`, `Abort`, `Release`, `Cancel`. Optional `Amount` for partial operations.

Returns `InstructionResponse` -- `InstructionType`, `Date`.

---

### `client.ThreeDs` -- `IElavonThreeDsService`

| Method | Description |
|---|---|
| `Initialise3DsAsync(transactionId, request)` | Begin a 3DS challenge for a transaction in `3DAuth` / `ChallengeRequired` status |
| `Complete3DsAsync(transactionId, request)` | Complete the challenge after receiving the CRes from the issuer ACS |

`Initialise3DsAsync` -> `Initialise3DsResponse` -- `Status`, `AcsUrl`, `CReq`.
`Complete3DsAsync` -> `Complete3DsResponse` -- `TransactionId`, `Status`.

---

### `client.Tokens` -- `IElavonTokensService`

| Method | Description |
|---|---|
| `CreateTokenAsync(request)` | Tokenise a card for future use without processing a transaction |
| `PayWithTokenAsync(request)` | Process a payment using a previously stored card token |

`CreateTokenAsync` -> `CreateTokenResponse` -- `Token`.
`PayWithTokenAsync` -> `PaymentResponse`.

---

### `client.Wallets` -- `IElavonWalletsService`

| Method | Description |
|---|---|
| `CreateMerchantSessionKeyAsync(request)` | Create a merchant session key for drop-in card field use |
| `ValidateMerchantSessionKeyAsync(request)` | Validate whether an existing MSK is still active |
| `CreateApplePaySessionAsync(request)` | Obtain an Apple Pay merchant session for `completeMerchantValidation` |

---

### `client.CardIdentifiers` -- `IElavonCardIdentifiersService`

| Method | Description |
|---|---|
| `CreateCardIdentifierAsync(merchantSessionKey, request)` | Tokenise card details against an MSK; uses Bearer auth |
| `LinkCardIdentifierAsync(cardIdentifier, request)` | Link a security code (CVV) to an existing card identifier |

`CreateCardIdentifierAsync` -> `CreateCardIdentifierResponse` -- `CardIdentifier`, `Expiry`, `CardType`.
`LinkCardIdentifierAsync` -> `Task` (no response body).

---

## Architecture Overview

```
src/
+-- ElavonPaymentsNet/
    +-- ElavonPaymentsClient.cs              # Entry point -- exposes all service groups
    +-- ElavonPaymentsClientOptions.cs       # Configuration (key, password, environment, timeout)
    +-- Http/
    |   +-- ElavonApiClient.cs               # All HTTP, auth, serialisation, error mapping (internal)
    |   +-- ElavonResilienceHandler.cs       # Central retry policy (GET-only, safe-by-default)
    |   +-- ElavonEnvironment.cs             # Sandbox / Live enum
    +-- Interfaces/
    |   +-- IElavonTransactionService.cs
    |   +-- IElavonPostPaymentService.cs
    |   +-- IElavonThreeDsService.cs
    |   +-- IElavonTokensService.cs
    |   +-- IElavonWalletsService.cs
    |   +-- IElavonCardIdentifiersService.cs
    |   +-- IElavonInstructionsService.cs
    +-- Services/
    |   +-- ElavonTransactionService.cs
    |   +-- ElavonPostPaymentService.cs
    |   +-- ElavonThreeDsService.cs
    |   +-- ElavonTokensService.cs
    |   +-- ElavonWalletsService.cs
    |   +-- ElavonCardIdentifiersService.cs
    |   +-- ElavonInstructionsService.cs
    +-- Models/
    |   +-- Public/
    |   |   +-- Requests/                    # All public request models
    |   |   +-- Responses/                   # All public response models
    |   |   +-- TransactionType.cs
    |   |   +-- InstructionType.cs
    |   +-- Internal/
    |       +-- ApiErrorResponse.cs          # API error payload (internal)
    |       +-- Dto/ApiDtos.cs               # Wire-format DTOs (internal)
    +-- Mapping/
    |   +-- RequestMapper.cs                 # Injects transactionType string into DTOs (internal)
    +-- Exceptions/
    |   +-- ElavonApiException.cs
    |   +-- ElavonAuthenticationException.cs
    +-- Extensions/
        +-- ServiceCollectionExtensions.cs   # AddElavonPayments(...)

tests/
+-- ElavonPaymentsNet.Tests/
    +-- Client/
    |   +-- ElavonPaymentsClientTests.cs
    +-- Http/
    |   +-- ElavonResilienceHandlerTests.cs
    |   +-- ElavonResilienceHandlerRetryTests.cs
    |   +-- Fakes/
    |       +-- FakeHttpMessageHandler.cs
    +-- Mapping/
    |   +-- RequestMapperTests.cs
    +-- Services/
        +-- ElavonServicesTests.cs
```

The internal `ElavonApiClient` is the only class that touches `HttpClient`. Services are thin orchestrators: map request -> call `ElavonApiClient` -> return typed response. No logic leaks between layers.

---

## Tech Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (`net10.0`) |
| Language | C# -- nullable enabled, implicit usings |
| HTTP | `Microsoft.Extensions.Http` / `IHttpClientFactory` |
| Serialisation | `System.Text.Json` -- camelCase, nulls omitted |
| DI | `Microsoft.Extensions.DependencyInjection.Abstractions` |
| Testing | xUnit |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- An Opayo sandbox or live account

### Clone and run

```bash
git clone https://github.com/untiedshoes/ElavonPaymentsNet.git
cd ElavonPaymentsNet
dotnet build
dotnet test
```

---

## Testing

Unit tests cover:

- `RequestMapper` -- transactionType injection for all four `TransactionType` values, token wrapping, billing address mapping
- `ElavonPaymentsClient` constructor guard clauses -- blank credentials throw `ArgumentException`
- Environment URL resolution -- sandbox and live base URLs resolve correctly
- `ElavonResilienceHandler` -- retries GET transient faults only, never retries POST, no retry on 4xx
- Service-layer orchestration -- verifies route, verb, auth mode, and core payload mapping across all service methods

`InternalsVisibleTo` exposes internal mappers and DTOs to the test project -- mapping is verified directly without going through HTTP. No live API calls required.

`FakeHttpMessageHandler` is used in HTTP and service unit tests to keep tests fast, deterministic, and isolated. It allows each test to simulate exact API responses, failures, and cancellation behavior through `HttpClient` without real network calls.

```bash
dotnet test
```

Integration tests against the Opayo sandbox are planned for a future phase.

---

## Notes

- Amounts must be in the **smallest currency unit** (pence for GBP, cents for EUR/USD).
- `VendorTxCode` must be unique per transaction within your vendor account.
- All operations are `async` -- sync-over-async is not supported.
- `ElavonEnvironment.Sandbox` is the default. Set `Environment = ElavonEnvironment.Live` explicitly for production.
- `CancellationToken` is propagated through to `HttpClient.SendAsync` on every call.

---

## Roadmap

- Integration tests against the Opayo sandbox
- `RetrieveTransactionAsync` -- fetch a transaction by ID
- Expanded 3DS metadata (SCA fields, exemption indicators)
- NuGet package publication

### Testing Gaps to Close

- Contract tests: validate SDK request/response JSON shapes against `docs/schema/*` and `docs/sdk-surface.yaml`
- Integration tests: add sandbox-backed tests with environment-variable gating and explicit skip behavior when credentials are not configured
- Smoke tests: add a minimal fast sanity suite for CI (SDK bootstrapping + one safe end-to-end API flow)

---

## References

- [Opayo PI REST API Reference](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)
- [Microsoft.Extensions.Http](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)

---

## License

MIT -- see [LICENSE](LICENSE)

---

## Author

Craig Richards -- Backend Developer | .NET Engineer
