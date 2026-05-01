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

| Exception | Status | When thrown |
|---|---|---|
| `ElavonAuthenticationException` | 401 | Invalid or missing credentials |
| `ElavonValidationException` | 400 | Malformed or rejected request |
| `ElavonPaymentDeclinedException` | 402 | Card declined by the issuer |
| `ElavonRateLimitException` | 429 | Too many requests; exposes `RetryAfter` (`TimeSpan?`) |
| `ElavonServerException` | 5xx | Server-side fault |
| `ElavonApiException` | any | Base class; catch-all for unmapped status codes |

All exceptions expose `HttpStatusCode` (as `int`), `RawResponse` (original API body), and optionally `ErrorCode`. All are catchable as `ElavonApiException`.

```csharp
try
{
    var result = await client.Transactions.CreateTransactionAsync(request);
}
catch (ElavonPaymentDeclinedException)
{
    // Card declined -- present to the user, do not retry
}
catch (ElavonRateLimitException ex)
{
    // Respect the Retry-After window before retrying
    if (ex.RetryAfter.HasValue)
        await Task.Delay(ex.RetryAfter.Value, cancellationToken);
}
catch (ElavonValidationException ex)
{
    // Malformed request -- inspect ex.ErrorCode and ex.RawResponse
}
catch (ElavonAuthenticationException)
{
    // Invalid integration key or password
}
catch (ElavonServerException)
{
    // 5xx -- unknown state on POST; query by VendorTxCode before retrying
}
catch (ElavonApiException ex)
{
    Console.WriteLine($"{ex.HttpStatusCode}: {ex.ErrorCode}");
}
```

---

## Resilience and Retry Safety

Retry logic lives in a single `DelegatingHandler` — the only place in the SDK it is implemented. The pipeline in order:

```
ElavonLoggingHandler → ElavonAuthenticationHandler → ElavonResilienceHandler → HttpClientHandler
```

- **GET requests** are retried automatically (up to `MaxRetryAttempts`, default `3`) with exponential backoff and jitter.
- **POST requests are never retried** — a payment that may have executed must not be sent again without reconciliation.
- The Opayo API has no `Idempotency-Key` mechanism, so POST retry cannot be made safe by the SDK.

> For the full retry policy, failure semantics, idempotency limitation, unknown-state recovery patterns, and production flow guidance, see **[RETRYING_AND_RELIABILITY.md](RETRYING_AND_RELIABILITY.md)**.

---

## SDK Service Reference

All methods are `async` and accept a `CancellationToken`. Each service group has a corresponding interface.

**`client.Transactions` -- `IElavonTransactionService`**

| Method | Description |
|---|---|
| `CreateTransactionAsync(request)` | Create a transaction -- `TransactionType` determines behaviour: `Payment`, `Authorise`, `Deferred`, or `Repeat` |
| `RetrieveTransactionAsync(transactionId)` | Retrieve a previously created transaction by its Elavon transaction ID |

Returns `PaymentResponse` -- `TransactionId`, `Status`, `StatusCode`, `StatusDetail`, `ThreeDSecure`.

---

**`client.PostPayments` -- `IElavonPostPaymentService`**

| Method | Description |
|---|---|
| `CaptureTransactionAsync(transactionId, request)` | Capture a deferred or authorised payment |
| `RefundTransactionAsync(transactionId, request)` | Refund a settled payment, fully or partially |
| `VoidTransactionAsync(transactionId)` | Void a payment that has not yet settled |

Returns `PostPaymentResponse` -- `TransactionId`, `Status`.

---

**`client.Instructions` -- `IElavonInstructionsService`**

| Method | Description |
|---|---|
| `CreateInstructionAsync(transactionId, request)` | Post a lifecycle instruction to a transaction |

`InstructionType` values: `Void`, `Abort`, `Release`, `Cancel`. Optional `Amount` for partial operations.

Returns `InstructionResponse` -- `InstructionType`, `Date`.

---

**`client.ThreeDs` -- `IElavonThreeDsService`**

| Method | Description |
|---|---|
| `Initialise3DsAsync(transactionId, request)` | Begin a 3DS challenge for a transaction in `3DAuth` / `ChallengeRequired` status |
| `Complete3DsAsync(transactionId, request)` | Complete the challenge after receiving the CRes from the issuer ACS |

`Initialise3DsAsync` -> `Initialise3DsResponse` -- `Status`, `AcsUrl`, `CReq`.
`Complete3DsAsync` -> `Complete3DsResponse` -- `TransactionId`, `Status`.

---

**`client.Tokens` -- `IElavonTokensService`**

| Method | Description |
|---|---|
| `CreateTokenAsync(request)` | Tokenise a card for future use without processing a transaction |
| `PayWithTokenAsync(request)` | Process a payment using a previously stored card token |

`CreateTokenAsync` -> `CreateTokenResponse` -- `Token`.
`PayWithTokenAsync` -> `PaymentResponse`.

---

**`client.Wallets` -- `IElavonWalletsService`**

| Method | Description |
|---|---|
| `CreateMerchantSessionKeyAsync(request)` | Create a merchant session key for drop-in card field use |
| `ValidateMerchantSessionKeyAsync(request)` | Validate whether an existing MSK is still active |
| `CreateApplePaySessionAsync(request)` | Obtain an Apple Pay merchant session for `completeMerchantValidation` |

---

**`client.CardIdentifiers` -- `IElavonCardIdentifiersService`**

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
    +-- ElavonPaymentsClientOptions.cs       # Configuration (key, password, environment, timeout, retries)
    +-- Http/
    |   +-- ElavonApiClient.cs               # Serialisation, error mapping, HTTP dispatch (internal)
    |   +-- ElavonAuthenticationHandler.cs   # Basic / Bearer auth injection (DelegatingHandler)
    |   +-- ElavonLoggingHandler.cs          # Request/response debug logging (DelegatingHandler)
    |   +-- ElavonResilienceHandler.cs       # GET-only retry with exponential backoff (DelegatingHandler)
    |   +-- ElavonRequestContext.cs          # HttpRequestOptionsKey for bearer token pass-through
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
    |   +-- ElavonApiException.cs            # Base exception (HttpStatusCode, RawResponse, ErrorCode)
    |   +-- ElavonAuthenticationException.cs # 401
    |   +-- ElavonValidationException.cs     # 400
    |   +-- ElavonPaymentDeclinedException.cs# 402
    |   +-- ElavonRateLimitException.cs      # 429 -- exposes RetryAfter (TimeSpan?)
    |   +-- ElavonServerException.cs         # 5xx
    +-- Extensions/
        +-- ServiceCollectionExtensions.cs   # AddElavonPayments(...)

tests/
+-- ElavonPaymentsNet.Tests/
    +-- Client/
    |   +-- ElavonPaymentsClientTests.cs
    +-- Http/
    |   +-- ElavonApiClientExceptionTests.cs # Typed exception hierarchy, Retry-After parsing, empty/malformed 2xx body
    |   +-- ElavonResilienceHandlerTests.cs
    |   +-- ElavonResilienceHandlerRetryTests.cs
    |   +-- Fakes/
    |       +-- FakeHttpMessageHandler.cs
    +-- Contract/
    |   +-- SchemaContractTests.cs
    +-- Integration/
    |   +-- ElavonIntegrationTests.cs
    +-- Mapping/
    |   +-- RequestMapperTests.cs
    +-- Services/
        +-- ElavonServicesTests.cs
```

The internal `ElavonApiClient` is the only class that touches `HttpClient`. Auth, logging, and retry are each handled by a dedicated `DelegatingHandler` in the pipeline — `ElavonApiClient` sees only the final response. Services are thin orchestrators: map request → call `ElavonApiClient` → return typed response. No logic leaks between layers.

---

## Design Decisions

### Why this SDK has no domain validators

The SDK intentionally does not validate request payloads before sending them to the API. This is a deliberate design choice, not an oversight.

**1. Different responsibility boundary**  
This SDK's job is transport orchestration — authentication, serialisation, routing, and error mapping. Domain validation belongs to an Application or Domain layer in the calling service. Mixing those concerns here would blur the SDK's responsibility.

**2. Opayo is the canonical source of truth**  
Required and conditional field rules are gateway-defined and change between API versions. Duplicating those rules locally risks drift: the SDK could reject requests the API would accept, or accept requests the API now rejects.

**3. Avoid false negatives in a payment context**  
Over-strict local validators can silently block valid transactions. In financial integrations a false rejection is a worse outcome than delegating validation to the provider and surfacing a typed `ElavonApiException` with the precise `ErrorCode`. **Nobody wants to be debugging a silent SDK rejection at 2am, only to discover the gateway would have happily accepted the request all along.**

**4. Stripe-style thin client**  
The design follows the Stripe SDK model — typed request/response models with minimal client-side guard rails. Server-side validation is canonical; the SDK exposes errors precisely via `ErrorCode` and `RawResponse`.

**When client-side guards are appropriate**  
A small number of invariant guards are enforced — values that will never be valid under any Opayo configuration:

- Blank/missing `IntegrationKey` or `IntegrationPassword` → `ArgumentException` at construction time  
- Invalid `MaxRetryAttempts` range (outside 1–10) → `ArgumentOutOfRangeException` at handler construction  

These are permanent SDK-level invariants, not gateway business rules.

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

## Playground (Sandbox Purchase)

A standalone user-style playground is available outside the SDK source in `playground/ElavonPaymentsNet.Playground`.
It references the SDK project and exercises a real purchase call through `ElavonPaymentsClient`.

### Configure environment variables

```bash
export ELAVON_INTEGRATION_KEY="your-sandbox-integration-key"
export ELAVON_INTEGRATION_PASSWORD="your-sandbox-integration-password"

# Optional overrides (defaults are set in Program.cs)
export ELAVON_TEST_CARD_NUMBER="4929000000006"
export ELAVON_TEST_CARD_EXPIRY="1229"
export ELAVON_TEST_CARD_CVV="123"
export ELAVON_TEST_CARDHOLDER="Sandbox Tester"
```

### Run playground purchase test

```bash
dotnet run --project playground/ElavonPaymentsNet.Playground/ElavonPaymentsNet.Playground.csproj
```

### Interactive prompts

When the playground starts, it prompts for:

- Card number, expiry, CVV, and cardholder name (press Enter to accept defaults)

Environment variables are used as the default values for each prompt if set.

Useful references:

- Sandbox accounts: https://developer.elavon.com/products/en-uk/opayo/v1/api-reference#tag/Sandbox-Accounts
- Test cards: https://developer.elavon.com/products/en-uk/opayo/v1/api-reference#tag/Test-Card-Details

---

## Testing

Unit tests cover:

- `RequestMapper` -- transactionType injection for all four `TransactionType` values, token wrapping, billing address mapping
- `ElavonPaymentsClient` constructor guard clauses -- blank credentials, null `ILoggerFactory`, and null `HttpClient` all throw at construction time
- Environment URL resolution -- sandbox and live base URLs resolve correctly
- `ElavonResilienceHandler` -- retries GET transient faults only, never retries POST, no retry on 4xx
- `ElavonApiClient` error mapping -- all HTTP status code paths, Retry-After header parsing, and empty/malformed 2xx body deserialization
- Service-layer orchestration -- verifies route, verb, auth mode, and core payload mapping across all service methods
- Path parameter validation and URI encoding -- blank path parameters throw `ArgumentException`; special characters are percent-encoded before dispatch
- Input guard coverage -- blank `merchantSessionKey` throws before the HTTP call is made

`InternalsVisibleTo` exposes internal mappers and DTOs to the test project -- mapping is verified directly without going through HTTP. No live API calls required.

`FakeHttpMessageHandler` is used in HTTP and service unit tests to keep tests fast, deterministic, and isolated. It allows each test to simulate exact API responses, failures, and cancellation behavior through `HttpClient` without real network calls.

```bash
dotnet test
```

Manual integration tests are available and gated by environment variables (`ELAVON_INTEGRATION_KEY`, `ELAVON_INTEGRATION_PASSWORD`, `ELAVON_SAFE_TRANSACTION_ID`). They are intended for safe sandbox verification and are not run by default in local test runs.

---

## Notes

- Amounts must be in the **smallest currency unit** (pence for GBP, cents for EUR/USD).
- `VendorTxCode` must be unique per transaction within your vendor account.
- All operations are `async` -- sync-over-async is not supported.
- `ElavonEnvironment.Sandbox` is the default. Set `Environment = ElavonEnvironment.Live` explicitly for production.
- `CancellationToken` is propagated through to `HttpClient.SendAsync` on every call.

### Environment Base URLs

This SDK resolves the server base URL from `ElavonEnvironment`.

- `Sandbox` -> `https://sandbox.opayo.eu.elavon.com/api/v1`
- `Live` -> `https://live.opayo.eu.elavon.com/api/v1`

Legacy hostnames may still appear in older Opayo materials (`pi-test.sagepay.com`, `pi-live.sagepay.com`), but this SDK intentionally defaults to the Elavon domains above.

You do **not** need to pass an environment server URL in normal usage. Set `Environment` and the SDK resolves the correct base URL internally.

Use a custom server URL only if you explicitly need non-default routing (for example, a migration or compatibility scenario), and manage that override at your own integration boundary.

---

## Roadmap

- Integration tests against the Opayo sandbox
- Expanded 3DS metadata (SCA fields, exemption indicators)
- NuGet package publication

### Playground Coverage to Add

#### Phase 1 (Core Post-Payment Flows)

- Scenario presets: terminal menu for common sandbox test-card outcomes
- Capture flow: authorise/deferred payment then `CaptureTransactionAsync`
- Refund flow: create payment then `RefundTransactionAsync` (full and partial)
- Void flow: create payment then `VoidTransactionAsync` before settlement

#### Phase 2 (Advanced Flows)

- 3DS playground script: `Initialise3DsAsync` and `Complete3DsAsync` path testing
- Token flow playground: `CreateTokenAsync` then `PayWithTokenAsync`
- Card identifier playground: `CreateMerchantSessionKeyAsync` + `CreateCardIdentifierAsync` + payment with token

### Testing Gaps to Close

- **Integration breadth**: expand sandbox-backed scenarios (additional happy paths and controlled failure paths) using environment-variable gating and explicit skip behavior when credentials are absent
- **Smoke tests**: a minimal fast CI suite covering SDK bootstrapping and one safe end-to-end API flow (e.g. `GET /merchant-session-keys` validation call)

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
