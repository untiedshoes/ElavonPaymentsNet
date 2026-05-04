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
- Strong Customer Authentication (EMV 3DS v2) request support via `StrongCustomerAuthentication` on transaction requests
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

## Additional Guides

- [SDK_CONSUMER_GUIDE.md](SDK_CONSUMER_GUIDE.md) - full consumer integration and usage guide with end-to-end examples
- [AI_TOOLING_PLAYGROUND.md](AI_TOOLING_PLAYGROUND.md) - operational runbook for AI tooling driving the interactive playground

## SDK Surface Contract (Source of Truth)

The file [docs/sdk-surface.yaml](docs/sdk-surface.yaml) is the canonical SDK surface contract (sometimes referred to as `sdk-surface.yml`).

It is used to define, in one place:

- Operation groups, HTTP methods, and routes
- Request/response DTO intent per operation
- Contract fixtures in [docs/schema](docs/schema)
- Validation profile intent and flow notes (for example, 3DS challenge-only completion)

Why we keep it:

- Prevents drift between service implementations, schema fixtures, tests, and documentation
- Makes endpoint-level behavior explicit for review and tooling
- Provides a stable, human-readable contract for future automation and codegen-style checks

How it is used in this repository:

- As the documentation and review baseline when adding or changing SDK operations
- As a contract map for schema contract tests and fixture updates
- As an implementation alignment checklist for `Services`, `Models`, and test coverage

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

TransactionType overview for `POST /transactions`:

- `Payment`: auth + capture in one step.
- `Deferred`: hold now, release/capture later.
- `Authenticate`: cardholder verification only, no funds reserved.
- `Repeat`: reuse previously captured customer/card details.
- `Refund`: return funds against a prior transaction reference.
- `Authorise`: authorisation transaction for later capture.

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
     | (.Status="3DAuth",    |                             |
     |  .AcsUrl, .CReq)      |                             |
   |<----------------------|                             |
   |                       |                             |
    |  [redirect cardholder to acsUrl with cReq]         |
    |  [ACS authenticates, posts cRes to your callback]  |
    |                       |                             |
   | Complete3DsAsync(cRes)|  POST /transactions/{id}   |
     |---------------------->|       /3d-secure-challenge  |
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
   |  (Card={msk,cardId})  |  Authorization: Basic ...   |
   |---------------------->|---------------------------->|
   |                       |<----------------------------|
   | PaymentResponse       |                             |
   |<----------------------|                             |


---------------------------------------------------------------------------
POST-PAYMENT & INSTRUCTIONS
---------------------------------------------------------------------------

  After Authorise or Deferred:

    CaptureTransactionAsync   ->  POST /transactions/{id}/instructions (Release)
    RefundTransactionAsync    ->  POST /transactions (Refund + referenceTransactionId)
    VoidTransactionAsync      ->  POST /transactions/{id}/instructions (Void)

  Lifecycle instructions (Void, Abort, Release, Cancel):

  CreateInstructionAsync    ->  POST /transactions/{id}/instructions
```

---

## Quick Example

The Opayo PI API uses a **merchant session key + card identifier** flow. Card details are tokenised against a short-lived session key before the transaction is submitted — raw PAN is never sent to your server.

```csharp
var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
{
    IntegrationKey      = "your-integration-key",
    IntegrationPassword = "your-integration-password",
    Environment         = ElavonEnvironment.Sandbox
});

// Step 1: Obtain a short-lived merchant session key
var session = await client.Wallets.CreateMerchantSessionKeyAsync(
    new MerchantSessionRequest { VendorName = "your-vendor-name" });

// Step 2: Tokenise the card details against the session key
var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
    session.MerchantSessionKey,
    new CreateCardIdentifierRequest
    {
        CardDetails = new CardDetails
        {
            CardNumber     = "4929000000006",
            ExpiryDate     = "1229",
            SecurityCode   = "123",
            CardholderName = "Craig Richards"
        }
    });

// Step 3: Submit the transaction using the card identifier
var result = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType   = TransactionType.Payment,
    VendorTxCode      = "ORDER-001",
    Amount            = 1999,   // pence -- GBP 19.99
    Currency          = "GBP",
    Description       = "Widget purchase",
    CustomerFirstName = "Craig",
    CustomerLastName  = "Richards",
    PaymentMethod     = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = session.MerchantSessionKey,
            CardIdentifier     = cardId.CardIdentifier
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1   = "88",
        City       = "London",
        PostalCode = "412",
        Country    = "GB"
    },
    Apply3DSecure = Apply3DSecureOption.Disable,   // omit to use account default
    StrongCustomerAuthentication = new StrongCustomerAuthentication
    {
        NotificationURL             = "https://example.com/3ds-notify",
        BrowserIP                   = "203.0.113.10",
        BrowserAcceptHeader         = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        BrowserJavascriptEnabled    = true,
        BrowserJavaEnabled          = false,
        BrowserLanguage             = "en-GB",
        BrowserColorDepth           = "24",
        BrowserScreenHeight         = "1080",
        BrowserScreenWidth          = "1920",
        BrowserTZ                   = "0",
        BrowserUserAgent            = "Mozilla/5.0",
        ChallengeWindowSize         = "FullScreen",
        TransType                   = "GoodsAndServicePurchase",
        ThreeDSRequestorChallengeInd= "03",

        // Optional advanced metadata for issuer risk scoring / exemptions
        ThreeDSRequestorExemptionIndicator = "lowValue",
        MerchantRiskIndicator = new MerchantRiskIndicator
        {
            DeliveryEmailAddress = "shopper@example.com",
            DeliveryTimeframe = "01",
            ReorderItemsInd = "01"
        },
        ThreeDSRequestorPriorAuthenticationInfo = new ThreeDSRequestorPriorAuthenticationInfo
        {
            ThreeDSReqPriorAuthMethod = "02",
            ThreeDSReqPriorAuthTimestamp = "202605041030",
            ThreeDSReqPriorRef = "AUTH-REF-123"
        }
    }
});

Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Transaction: {result.TransactionId}");
```

Note: advanced indicator codes are acquirer/gateway specific. Use values defined in your Elavon account documentation.

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
| `ElavonValidationException` | 400, 422 | Malformed or rejected request |
| `ElavonPaymentDeclinedException` | 402 | Card declined by the issuer |
| `ElavonRateLimitException` | 429 | Too many requests; exposes `RetryAfter` (`TimeSpan?`) |
| `ElavonServerException` | 5xx | Server-side fault |
| `ElavonApiException` | any | Base class; catch-all for unmapped status codes |

All exceptions expose `HttpStatusCode` (as `int`), `RawResponse` (original API body), and optionally `ErrorCode`. All are catchable as `ElavonApiException`.

For a full production pattern (typed exceptions + `StatusKind`) and a complete `ElavonErrorCode` reference table, see [SDK Consumer Guide §11 Error Handling](SDK_CONSUMER_GUIDE.md#11-error-handling).

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

### Transaction Status Handling

The SDK keeps the raw API status string for forward compatibility (`Status`) and also exposes a typed view (`StatusKind`) on response models.

- `PaymentResponse.StatusKind`
- `Complete3DsResponse.StatusKind`
- `PostPaymentResponse.StatusKind`

Use `StatusKind` for branching logic and keep `Status` for logging/diagnostics.

```csharp
var payment = await client.Transactions.CreateTransactionAsync(request, cancellationToken);

switch (payment.StatusKind)
{
    case TransactionStatusKind.Ok:
        // Successful transaction
        break;

    case TransactionStatusKind.ThreeDAuth:
        // Redirect customer to ACS using payment.AcsUrl + payment.CReq
        break;

    case TransactionStatusKind.NotAuthed:
    case TransactionStatusKind.Rejected:
    case TransactionStatusKind.Invalid:
    case TransactionStatusKind.Malformed:
    case TransactionStatusKind.Error:
        // Business/validation error path
        break;

    case TransactionStatusKind.Unknown:
    default:
        // Future status introduced by API; handle conservatively and log raw payment.Status
        break;
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
| `CreateTransactionAsync(request)` | Create a transaction -- `TransactionType` determines behaviour: `Payment`, `Deferred`, `Authenticate`, `Repeat`, `Refund`, or `Authorise` |
| `RetrieveTransactionAsync(transactionId)` | Retrieve a previously created transaction by its Elavon transaction ID |

Returns `PaymentResponse`.

Key response fields include:

- Transaction result: `TransactionId`, `TransactionType`, `Status`, `StatusCode`, `StatusDetail`
- 3DS challenge handoff: `AcsUrl`, `CReq`, `AcsTransId`, `DsTransId`, `ThreeDSecure`
- Additional metadata (when returned): `AdditionalDeclineDetail`, `RetrievalReference`, `SettlementReferenceText`, `BankResponseCode`, `BankAuthorisationCode`, `AvsCvcCheck`, `PaymentMethod`, `Amount`, `Currency`, `FiRecipient`

Supported `TransactionType` semantics:

- `Payment`: most common purchase flow; effectively authorisation + capture in one step.
- `Deferred`: place a shadow/hold and capture later using `Release` (`client.PostPayments.CaptureTransactionAsync`).
- `Authenticate`: verify cardholder details without reserving funds; follow with an authorise/payment flow to take funds.
- `Repeat`: process another payment using customer/card details from a previous transaction.
- `Refund`: credit funds back to the customer for a previous transaction (partial/full up to original amount).
- `Authorise`: create an authorisation intended for subsequent capture/release.

`Refund` is typically performed through `client.PostPayments.RefundTransactionAsync`, which wraps this transaction pattern and sets reference fields consistently.

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
| `Complete3DsAsync(transactionId, request)` | Complete the 3DS v2 challenge after receiving the `cRes` from the issuer ACS callback |

3DS flow note:

- There is no separate initialise endpoint in this SDK
- `CreateTransactionAsync` returns `Status = "3DAuth"` plus `AcsUrl` and `CReq` on `PaymentResponse`
- `Complete3DsAsync` posts `cRes` to `/transactions/{transactionId}/3d-secure-challenge` and returns `Complete3DsResponse`

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
    |   |   +-- Apply3DSecureOption.cs
    |   |   +-- StrongCustomerAuthentication.cs
    |   |   +-- TransactionType.cs
    |   |   +-- InstructionType.cs
    |   +-- Internal/
    |       +-- ApiErrorResponse.cs          # API error payload (internal)
    |       +-- Dto/ApiDtos.cs               # Wire-format DTOs (internal)
    +-- Mapping/
    |   +-- RequestMapper.cs                 # Injects transactionType string into DTOs (internal)
    +-- Validation/
    |   +-- Guard.cs                         # Shared argument validation helpers for service-layer invariants
    +-- Exceptions/
    |   +-- ElavonApiException.cs            # Base exception (HttpStatusCode, RawResponse, ErrorCode)
    |   +-- ElavonAuthenticationException.cs # 401
    |   +-- ElavonValidationException.cs     # 400 / 422
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

docs/
+-- sdk-surface.yaml                         # Source-of-truth SDK surface contract (operations, routes, schemas)
```

The internal `ElavonApiClient` is the only class that touches `HttpClient`. Auth, logging, and retry are each handled by a dedicated `DelegatingHandler` in the pipeline — `ElavonApiClient` sees only the final response. Services are thin orchestrators: map request → call `ElavonApiClient` → return typed response. No logic leaks between layers.

`Guard` centralizes service argument invariants (for example, null request checks and path-identifier checks) so failures occur before route construction and are consistent across service groups.

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

For AI-assisted and automation-driven operation, use [AI_TOOLING_PLAYGROUND.md](AI_TOOLING_PLAYGROUND.md) as the authoritative runbook. It documents:

- Deterministic prompt input sequences
- Correct ACS simulator POST usage (`creq` over POST)
- Reliable `cRes` extraction workflow
- Known failure signatures and recovery steps
- Expected success criteria for completed 3DS runs

Recommended usage split:

- Human exploratory runs: this README section
- AI/automation runs and repeatable 3DS workflows: [AI_TOOLING_PLAYGROUND.md](AI_TOOLING_PLAYGROUND.md)

### Configure environment variables

```bash
export ELAVON_INTEGRATION_KEY="your-sandbox-integration-key"
export ELAVON_INTEGRATION_PASSWORD="your-sandbox-integration-password"
export ELAVON_VENDOR_NAME="sandbox"   # or sandboxEC for the Extra Checks profile

# Optional card overrides (defaults are set in Program.cs)
export ELAVON_TEST_CARD_NUMBER="4929000000006"
export ELAVON_TEST_CARD_EXPIRY="1229"
export ELAVON_TEST_CARD_CVV="123"
export ELAVON_TEST_CARDHOLDER="Sandbox Tester"
export ELAVON_MAGIC_CARDHOLDER="SUCCESSFUL"   # controls 3DS simulation outcome

# Optional 3DS / SCA overrides (playground has safe defaults)
export ELAVON_NOTIFICATION_URL="https://example.com/3ds-notify" # must be valid https URL
export ELAVON_BROWSER_IP="203.0.113.10"
export ELAVON_BROWSER_USER_AGENT="Mozilla/5.0"
export ELAVON_BROWSER_LANGUAGE="en-GB"
export ELAVON_APPLY_3DS="Disable"             # Disable / Force / UseMSPSetting
```

### Run playground purchase test

```bash
dotnet run --project playground/ElavonPaymentsNet.Playground/ElavonPaymentsNet.Playground.csproj
```

### Interactive prompts

When the playground starts, it prompts for:

- **Card number, expiry, CVV, cardholder name** — press Enter to accept defaults
- **Magic cardholder (3DS simulation)** — controls the sandbox 3DS outcome via the cardholder name on the card identifier. Defaults to `SUCCESSFUL` (frictionless OK). Other values: `NOTAUTH`, `CHALLENGE`, `PROOFATTEMPT`, `REJECT`, `TECHDIFFICULTIES`, `ERROR`.
- **Apply3DSecure override** — `Disable`, `Force`, `UseMSPSetting`, or blank to use account default.

Environment variables are used as the default values for each prompt if set.

### Forcing a visible 3DS challenge in the playground

Use these values to reliably trigger challenge status in sandbox testing:

- `Magic cardholder` = `CHALLENGE`
- `Apply3DSecure` = `Force`
- `ELAVON_NOTIFICATION_URL` must be a valid fully-qualified `https://` URL (do not use localhost)

### Sandbox billing address for AVS checks

The playground uses `address1: "88"` and `postalCode: "412"` — the values required to pass AVS checks on the Extra Checks profile (`sandboxEC`). These are Opayo magic test values and not real addresses.

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

Manual integration tests are available and run against the real Elavon/Opayo sandbox environment.

**Sandbox credentials are hardcoded** directly in `SandboxCredentials.cs` — these are the standard Opayo PI REST API documentation credentials, publicly available, and safe to commit. They only work against the non-production sandbox environment, never live. No environment variable setup is required to run integration tests.

```bash
# Run all tests including integration tests
dotnet test --filter "Category=Integration"
```

The only optional variable is `ELAVON_SAFE_TRANSACTION_ID`, which enables a read-only retrieve test against a known transaction ID in your sandbox account. All other integration tests are self-contained.

Integration tests are tagged `[Trait("Category", "Integration")]` and excluded from the default `dotnet test` run to keep local development fast.

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

- Expanded 3DS metadata beyond baseline browser fields (risk indicators, prior auth, exemptions)

### Playground Coverage to Add

- Scenario presets: terminal menu for common sandbox test-card outcomes
- Capture flow: authorise/deferred payment then `CaptureTransactionAsync`
- Refund flow: create payment then `RefundTransactionAsync` (full and partial)
- Void flow: create payment then `VoidTransactionAsync` before settlement
- Token flow: `CreateTokenAsync` then `PayWithTokenAsync`

### Testing Gaps to Close

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
