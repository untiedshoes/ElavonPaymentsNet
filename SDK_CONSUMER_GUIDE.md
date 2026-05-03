# ElavonPaymentsNet SDK Consumer Guide

This document is a practical, end-to-end guide for integrating and operating the ElavonPaymentsNet SDK in your own .NET applications.

It is intended to sit alongside:
- README.md
- RETRYING_AND_RELIABILITY.md

If you are new to the SDK, start here.

## 1. Prerequisites

- .NET 10 SDK
- Elavon/Opayo PI REST credentials:
  - Integration key
  - Integration password
- Network access to:
  - Sandbox: https://sandbox.opayo.eu.elavon.com/api/v1
  - Live: https://live.opayo.eu.elavon.com/api/v1

## 2. Install and Configure

### Direct client construction

```csharp
using ElavonPaymentsNet;
using ElavonPaymentsNet.Http;

var client = new ElavonPaymentsClient(new ElavonPaymentsClientOptions
{
    IntegrationKey = Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_KEY")!,
    IntegrationPassword = Environment.GetEnvironmentVariable("ELAVON_INTEGRATION_PASSWORD")!,
    Environment = ElavonEnvironment.Sandbox,
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetryAttempts = 3
});
```

### Dependency Injection registration

```csharp
using ElavonPaymentsNet.Extensions;
using ElavonPaymentsNet.Http;

builder.Services.AddElavonPayments(options =>
{
    options.IntegrationKey = builder.Configuration["Elavon:IntegrationKey"]!;
    options.IntegrationPassword = builder.Configuration["Elavon:IntegrationPassword"]!;
    options.Environment = ElavonEnvironment.Sandbox;
    options.Timeout = TimeSpan.FromSeconds(30);
    options.MaxRetryAttempts = 3;
});
```

## 3. Service Surface Overview

`ElavonPaymentsClient` exposes:

- `Transactions`
  - `CreateTransactionAsync`
  - `RetrieveTransactionAsync`
- `PostPayments`
  - `CaptureTransactionAsync`
  - `RefundTransactionAsync`
  - `VoidTransactionAsync`
- `Instructions`
  - `CreateInstructionAsync`
- `ThreeDs`
  - `Complete3DsAsync`
- `Wallets`
  - `CreateMerchantSessionKeyAsync`
  - `ValidateMerchantSessionKeyAsync`
  - `CreateApplePaySessionAsync`
- `CardIdentifiers`
  - `CreateCardIdentifierAsync`
  - `LinkCardIdentifierAsync`
- `Tokens`
  - `CreateTokenAsync`
  - `PayWithTokenAsync`

## 4. Transaction Flows

All transaction creation flows use `CreateTransactionRequest` and set `TransactionType`.

### 4.1 Payment

Use this for standard e-commerce charges where you want immediate authorisation (and capture behavior according to your account/acquirer setup).

```csharp
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

var payment = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Description = "Online order",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber = "4929000000006",
            ExpiryDate = "1229",
            SecurityCode = "123",
            CardholderName = "Sandbox Tester"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1 = "88",
        City = "London",
        PostalCode = "412",
        Country = "GB"
    },
    Apply3DSecure = Apply3DSecureOption.UseMSPSetting,
    StrongCustomerAuthentication = new StrongCustomerAuthentication
    {
        NotificationURL = "https://merchant.example.com/3ds-notify",
        BrowserIP = "203.0.113.10",
        BrowserAcceptHeader = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        BrowserJavascriptEnabled = true,
        BrowserJavaEnabled = false,
        BrowserLanguage = "en-GB",
        BrowserColorDepth = "24",
        BrowserScreenHeight = "1080",
        BrowserScreenWidth = "1920",
        BrowserTZ = "0",
        BrowserUserAgent = "Mozilla/5.0",
        ChallengeWindowSize = "FullScreen",
        TransType = "GoodsAndServicePurchase",
        ThreeDSRequestorChallengeInd = "03"
    }
});
```

### 4.2 Authorise

Use this when you need to reserve funds now and capture later (for example, ship-later or delayed-fulfilment flows).

```csharp
var authorise = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Authorise,
    VendorTxCode = $"AUTH-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 2500,
    Currency = "GBP",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber = "4929000000006",
            ExpiryDate = "1229",
            SecurityCode = "123",
            CardholderName = "Sandbox Tester"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1 = "88",
        City = "London",
        PostalCode = "412",
        Country = "GB"
    },
    StrongCustomerAuthentication = new StrongCustomerAuthentication
    {
        NotificationURL = "https://merchant.example.com/3ds-notify",
        BrowserIP = "203.0.113.10",
        BrowserAcceptHeader = "text/html,*/*",
        BrowserJavascriptEnabled = true,
        BrowserJavaEnabled = false,
        BrowserLanguage = "en-GB",
        BrowserColorDepth = "24",
        BrowserScreenHeight = "1080",
        BrowserScreenWidth = "1920",
        BrowserTZ = "0",
        BrowserUserAgent = "Mozilla/5.0",
        ChallengeWindowSize = "FullScreen",
        TransType = "GoodsAndServicePurchase",
        ThreeDSRequestorChallengeInd = "03"
    }
});
```

### 4.3 Deferred

Use this when you need a deferred transaction lifecycle and intend to release/capture at a later business step.

```csharp
var deferred = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Deferred,
    VendorTxCode = $"DEF-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 5000,
    Currency = "GBP",
    Description = "Deferred order",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber = "4929000000006",
            ExpiryDate = "1229",
            SecurityCode = "123",
            CardholderName = "Sandbox Tester"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1 = "88",
        City = "London",
        PostalCode = "412",
        Country = "GB"
    }
});
```

### 4.4 Repeat

Use this for subsequent charges based on a previous successful transaction reference, without collecting card details again (e.g. subscriptions, instalment payments, re-orders).

```csharp
var repeat = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Repeat,
    VendorTxCode = $"REP-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Description = "Recurring payment",
    RelatedTransactionId = "<original-successful-transaction-id>"
});
```

### 4.5 Authenticate

Use this when you want to authenticate the cardholder through 3DS without placing any charge. This is useful for verifying a card before saving it, or for SCA compliance checks prior to a deferred payment.

```csharp
var authenticate = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Authenticate,
    VendorTxCode = $"AUTH-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 0,
    Currency = "GBP",
    Description = "Card verification",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber = "4929000000006",
            ExpiryDate = "1229",
            SecurityCode = "123",
            CardholderName = "Sandbox Tester"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1 = "88",
        City = "London",
        PostalCode = "412",
        Country = "GB"
    },
    StrongCustomerAuthentication = new StrongCustomerAuthentication
    {
        NotificationURL = "https://merchant.example.com/3ds-notify",
        BrowserIP = "203.0.113.10",
        BrowserAcceptHeader = "text/html,*/*",
        BrowserJavascriptEnabled = true,
        BrowserJavaEnabled = false,
        BrowserLanguage = "en-GB",
        BrowserColorDepth = "24",
        BrowserScreenHeight = "1080",
        BrowserScreenWidth = "1920",
        BrowserTZ = "0",
        BrowserUserAgent = "Mozilla/5.0",
        ChallengeWindowSize = "FullScreen",
        TransType = "GoodsAndServicePurchase",
        ThreeDSRequestorChallengeInd = "03"
    }
});
```

### 4.6 Refund (via transaction)

Use this when creating a standalone refund transaction linked to a previous transaction. This is distinct from `PostPayments.RefundTransactionAsync`, which creates a post-payment refund instruction. Use the transaction-based refund when you need a new transaction record for the refund.

```csharp
var refund = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Refund,
    VendorTxCode = $"RFND-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Description = "Customer refund",
    RelatedTransactionId = "<original-successful-transaction-id>"
});
```

### 4.7 Retrieve a Transaction

Use this to reconcile transaction outcomes, refresh UI state, or diagnose post-payment statuses after asynchronous steps.

```csharp
var tx = await client.Transactions.RetrieveTransactionAsync("<transaction-id>");
Console.WriteLine($"Status={tx.Status} Code={tx.StatusCode}");
```

## 5. 3D Secure v2 Flow

There is no separate "initialise 3DS" endpoint in this SDK anymore.

### Step A: Create transaction

Call `CreateTransactionAsync` with complete `StrongCustomerAuthentication` data.

If 3DS challenge is required, the transaction response includes:
- `Status = "3DAuth"`
- `AcsUrl`
- `CReq`

### Step B: Redirect customer to ACS

POST form field `creq=<CReq>` to `AcsUrl`.

Important:
- Opening `AcsUrl` with GET returns HTTP 405.
- Use POST with form field `creq`.

### Step C: Receive ACS callback

ACS posts `cres` (lowercase) to your notification URL.

### Step D: Complete 3DS with SDK

```csharp
using ElavonPaymentsNet.Models.Public.Requests;

var complete = await client.ThreeDs.Complete3DsAsync(
    transactionId,
    new Complete3DsRequest
    {
        CRes = cresFromAcsCallback
    });

Console.WriteLine($"Status={complete.Status}");
Console.WriteLine($"3DS={complete.ThreeDSecure?.Status}");
```

## 6. Post-Payment Operations

### Capture (Release instruction under the hood)

```csharp
var capture = await client.PostPayments.CaptureTransactionAsync(
    transactionId,
    new CapturePaymentRequest { Amount = 1000 });
```

### Refund (new Refund transaction under the hood)

```csharp
var refund = await client.PostPayments.RefundTransactionAsync(
    transactionId,
    new RefundPaymentRequest
    {
        Amount = 1000,
        VendorTxCode = $"REF-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
        Description = "Customer refund"
    });
```

### Void

```csharp
var voidResult = await client.PostPayments.VoidTransactionAsync(transactionId);
```

## 7. Instructions API

```csharp
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;

var instruction = await client.Instructions.CreateInstructionAsync(
    transactionId,
    new InstructionRequest
    {
        InstructionType = InstructionType.Release,
        Amount = 1000
    });
```

Supported instruction types:
- `Void` — Cancel an authorised or deferred transaction before settlement. Use this if the order is cancelled before goods ship.
- `Abort` — Abort a deferred transaction that has not yet been released. Use when a deferred payment is no longer needed.
- `Release` — Capture/release a previously deferred or authorised amount. Use this to trigger settlement after fulfillment. An `Amount` can be specified to partially release.
- `Cancel` — Cancel a previously released instruction. Rarely used; refer to Elavon support for applicability.

## 8. Merchant Session Key and Card Identifiers

The card flow in Opayo uses two short-lived tokens that work together to keep raw card data off your server:

- **Merchant Session Key (MSK)** — A temporary key valid for 20 minutes. It opens a secure storage area on the Opayo side where a single set of card details can be held. Your frontend JavaScript (or your server during testing) uses this key to tokenise card details directly with Opayo, so the PAN never touches your application.
- **Card Identifier** — A temporary token (valid for ~400 seconds) that identifies a single card within the MSK's storage area. Once a card identifier exists, you pass the MSK + card identifier pair to `CreateTransactionAsync` instead of raw card details.

This two-token design is what makes the Opayo integration safe for non-PCI-compliant back-ends: only the tokens travel through your system, not the card number.

### Create merchant session key

Request a new MSK before rendering your payment form. Pass `VendorName` as your Opayo vendor name (or `"sandbox"` for testing).

```csharp
var msk = await client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest
{
    VendorName = "sandbox"
});

// msk.MerchantSessionKey is the token to pass to your frontend or to CreateCardIdentifierAsync
```

### Validate merchant session key

Before using an MSK that may have been sitting in session state for a while, you can check it is still valid. This avoids a confusing failure later at payment time.

```csharp
var check = await client.Wallets.ValidateMerchantSessionKeyAsync(new MerchantSessionValidationRequest
{
    MerchantSessionKey = msk.MerchantSessionKey!
});

// If the key has expired, request a fresh one and re-render the payment form.
```

### Create card identifier

Normally this step is performed on the client side via Opayo's JavaScript library (so the PAN never reaches your server). During testing or from a server-side integration, you can create the card identifier directly.

```csharp
var cardIdentifier = await client.CardIdentifiers.CreateCardIdentifierAsync(
    msk.MerchantSessionKey!,
    new CreateCardIdentifierRequest
    {
        CardDetails = new CardDetails
        {
            CardNumber = "4929000000006",
            ExpiryDate = "1229",
            SecurityCode = "123",
            CardholderName = "Sandbox Tester"
        }
    });

// cardIdentifier.CardIdentifier is the token to use in the payment request.
// It is valid for approximately 400 seconds.
```

### Optional: Link CVV after identifier creation

If you are reusing a saved card and want the customer to confirm their security code for additional safety, link the CVV to the existing card identifier (preferably from the client side).

```csharp
await client.CardIdentifiers.LinkCardIdentifierAsync(
    cardIdentifier.CardIdentifier!,
    new LinkCardIdentifierRequest { SecurityCode = "123" });
```

### Pay using card identifier

Pass the MSK and card identifier in `PaymentMethod.Card` instead of raw card details. Opayo resolves the stored card from the token pair at the point of charge.

```csharp
var paymentByIdentifier = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode = $"CARDID-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier = cardIdentifier.CardIdentifier
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1 = "88",
        City = "London",
        PostalCode = "412",
        Country = "GB"
    }
});
```

## 9. Tokens

### Create token

```csharp
var token = await client.Tokens.CreateTokenAsync(new CreateTokenRequest
{
    Card = new CardDetails
    {
        CardNumber = "4929000000006",
        ExpiryDate = "1229",
        SecurityCode = "123",
        CardholderName = "Sandbox Tester"
    }
});
```

### Pay with token

```csharp
var tokenPayment = await client.Tokens.PayWithTokenAsync(new PayWithTokenRequest
{
    VendorTxCode = $"TOK-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Token = token.Token!
});
```

## 10. Apple Pay Session

Before processing an Apple Pay payment, your backend must validate the merchant session with Apple. The Apple Pay JS API on the frontend will fire an `onvalidatemerchant` event containing a `validationURL`. Your backend must call this endpoint (via Opayo) and return the session object to the browser within 30 seconds.

```csharp
// Called from your backend when frontend fires onvalidatemerchant
var applePaySession = await client.Wallets.CreateApplePaySessionAsync(new ApplePaySessionRequest
{
    ValidationUrl = "https://apple-pay-gateway.apple.com/paymentservices/startSession",
    DomainName = "merchant.example.com"
});

// Return applePaySession.MerchantSession to your frontend
// The frontend passes it to appleSession.completeMerchantValidation(merchantSession)
```

Once the merchant session is validated, the customer authorises the payment on their device. The Apple Pay JS API returns a payment token, which your frontend sends to your backend. You then submit it as a `Payment` transaction with `PaymentMethod.ApplePayPaymentToken`.

> Note: Your domain must be registered with Opayo and Apple, and you must serve the `apple-developer-merchantid-domain-association` file at `/.well-known/` on that domain.

## 11. Error Handling

Use specific SDK exceptions where possible:

```csharp
using ElavonPaymentsNet.Exceptions;

try
{
    var result = await client.Transactions.CreateTransactionAsync(request);
}
catch (ElavonValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{error.Property}: {error.Description} ({error.Code})");
    }
}
catch (ElavonAuthenticationException ex)
{
    Console.WriteLine($"Auth failed ({ex.HttpStatusCode}): {ex.RawResponse}");
}
catch (ElavonRateLimitException ex)
{
    Console.WriteLine($"Rate limited ({ex.HttpStatusCode})");
}
catch (ElavonPaymentDeclinedException ex)
{
    Console.WriteLine($"Declined: {ex.RawResponse}");
}
catch (ElavonApiException ex)
{
    Console.WriteLine($"API error ({ex.HttpStatusCode}) code={ex.ErrorCode}\n{ex.RawResponse}");
}
```

## 12. Operational Guidance

- Treat `VendorTxCode` as an idempotency/business key in your own system.
- Never retry POST calls yourself without careful deduplication logic.
- Always send complete SCA browser data for card-not-present flows.
- Persist transaction IDs and important response fields (`statusCode`, `bankResponseCode`, `retrievalReference`) for support and reconciliation.
- Keep sandbox and live credentials strictly separated.

## 13. Minimal End-to-End Example

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 100,
    Currency = "GBP",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber = "4929000000006",
            ExpiryDate = "1229",
            SecurityCode = "123",
            CardholderName = "Sandbox Tester"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1 = "88",
        City = "London",
        PostalCode = "412",
        Country = "GB"
    },
    StrongCustomerAuthentication = new StrongCustomerAuthentication
    {
        NotificationURL = "https://merchant.example.com/3ds-notify",
        BrowserIP = "203.0.113.10",
        BrowserAcceptHeader = "text/html,*/*",
        BrowserJavascriptEnabled = true,
        BrowserJavaEnabled = false,
        BrowserLanguage = "en-GB",
        BrowserColorDepth = "24",
        BrowserScreenHeight = "1080",
        BrowserScreenWidth = "1920",
        BrowserTZ = "0",
        BrowserUserAgent = "Mozilla/5.0",
        ChallengeWindowSize = "FullScreen",
        TransType = "GoodsAndServicePurchase",
        ThreeDSRequestorChallengeInd = "03"
    }
});

if (response.Status == "3DAuth")
{
    // Redirect to response.AcsUrl with creq=response.CReq,
    // then call Complete3DsAsync with cRes from ACS callback.
}
```

## 14. Recommended Production Architecture (Controller/Service/Webhook)

For production systems, separate payment orchestration from HTTP endpoints.

Recommended split:
- Controller: HTTP request/response mapping only.
- Application service: business workflow and SDK orchestration.
- Webhook/callback controller: receives ACS callback and completes 3DS.
- State store: persists checkout/payment state (`Pending3Ds`, `Authorised`, `Failed`).

### 14.1 Contracts

```csharp
public sealed record StartCheckoutRequest(int Amount, string Currency, string CustomerEmail);

public sealed record StartCheckoutResult(
    string TransactionId,
    string Status,
    string? AcsUrl,
    string? CReq);

public interface ICheckoutPaymentService
{
    Task<StartCheckoutResult> StartCheckoutAsync(StartCheckoutRequest request, CancellationToken ct = default);
    Task<Complete3DsResponse> Complete3DsAsync(string transactionId, string cRes, CancellationToken ct = default);
}
```

### 14.2 Service layer (SDK orchestration)

```csharp
using ElavonPaymentsNet;
using ElavonPaymentsNet.Models.Public;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

public sealed class CheckoutPaymentService : ICheckoutPaymentService
{
    private readonly ElavonPaymentsClient _client;
    private readonly ICheckoutStateStore _store;

    public CheckoutPaymentService(ElavonPaymentsClient client, ICheckoutStateStore store)
    {
        _client = client;
        _store = store;
    }

    public async Task<StartCheckoutResult> StartCheckoutAsync(StartCheckoutRequest request, CancellationToken ct = default)
    {
        var tx = await _client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
        {
            TransactionType = TransactionType.Payment,
            VendorTxCode = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Amount = request.Amount,
            Currency = request.Currency,
            CustomerEmail = request.CustomerEmail,
            PaymentMethod = new PaymentMethod
            {
                Card = new CardDetails
                {
                    CardNumber = "4929000000006",
                    ExpiryDate = "1229",
                    SecurityCode = "123",
                    CardholderName = "Sandbox Tester"
                }
            },
            BillingAddress = new BillingAddress
            {
                Address1 = "88",
                City = "London",
                PostalCode = "412",
                Country = "GB"
            },
            Apply3DSecure = Apply3DSecureOption.UseMSPSetting,
            StrongCustomerAuthentication = new StrongCustomerAuthentication
            {
                NotificationURL = "https://merchant.example.com/webhooks/opayo/3ds",
                BrowserIP = "203.0.113.10",
                BrowserAcceptHeader = "text/html,*/*",
                BrowserJavascriptEnabled = true,
                BrowserJavaEnabled = false,
                BrowserLanguage = "en-GB",
                BrowserColorDepth = "24",
                BrowserScreenHeight = "1080",
                BrowserScreenWidth = "1920",
                BrowserTZ = "0",
                BrowserUserAgent = "Mozilla/5.0",
                ChallengeWindowSize = "FullScreen",
                TransType = "GoodsAndServicePurchase",
                ThreeDSRequestorChallengeInd = "03"
            }
        }, ct);

        if (string.IsNullOrWhiteSpace(tx.TransactionId))
            throw new InvalidOperationException("TransactionId was not returned.");

        await _store.SaveAsync(new CheckoutState
        {
            TransactionId = tx.TransactionId,
            Status = tx.Status ?? "Unknown",
            CreatedUtc = DateTimeOffset.UtcNow
        }, ct);

        return new StartCheckoutResult(
            tx.TransactionId,
            tx.Status ?? "Unknown",
            tx.AcsUrl,
            tx.CReq);
    }

    public async Task<Complete3DsResponse> Complete3DsAsync(string transactionId, string cRes, CancellationToken ct = default)
    {
        var result = await _client.ThreeDs.Complete3DsAsync(
            transactionId,
            new Complete3DsRequest { CRes = cRes },
            ct);

        await _store.Mark3DsCompletedAsync(
            transactionId,
            result.Status ?? "Unknown",
            result.StatusDetail,
            ct);

        return result;
    }
}
```

### 14.3 API controller for checkout start

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController : ControllerBase
{
    private readonly ICheckoutPaymentService _service;

    public CheckoutController(ICheckoutPaymentService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartCheckoutRequest request, CancellationToken ct)
    {
        var result = await _service.StartCheckoutAsync(request, ct);

        if (result.Status == "3DAuth")
        {
            return Ok(new
            {
                requires3Ds = true,
                transactionId = result.TransactionId,
                acsUrl = result.AcsUrl,
                cReq = result.CReq
            });
        }

        return Ok(new
        {
            requires3Ds = false,
            transactionId = result.TransactionId,
            status = result.Status
        });
    }
}
```

### 14.4 Complete 3DS callback handler (webhook endpoint)

This endpoint receives ACS POST data (form field `cres`) and completes 3DS in the SDK.

```csharp
using ElavonPaymentsNet.Exceptions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("webhooks/opayo/3ds")]
public sealed class ThreeDsWebhookController : ControllerBase
{
    private readonly ICheckoutPaymentService _service;
    private readonly ICheckoutStateStore _store;
    private readonly ILogger<ThreeDsWebhookController> _logger;

    public ThreeDsWebhookController(
        ICheckoutPaymentService service,
        ICheckoutStateStore store,
        ILogger<ThreeDsWebhookController> logger)
    {
        _service = service;
        _store = store;
        _logger = logger;
    }

    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Receive([FromForm(Name = "cres")] string? cres, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cres))
            return BadRequest("Missing form field 'cres'.");

        // Resolve your transaction ID from your own checkout state.
        // Common approaches:
        // 1) Correlate by browser session / one-time state token.
        // 2) Decode CRes and map by your persisted threeDSServerTransID.
        var pending = await _store.GetMostRecentPending3DsAsync(ct);
        if (pending is null)
            return NotFound("No pending 3DS transaction found.");

        try
        {
            var result = await _service.Complete3DsAsync(pending.TransactionId, cres, ct);
            _logger.LogInformation("3DS completed for {TransactionId} with status {Status}", pending.TransactionId, result.Status);

            // Respond with 200 so ACS/browser considers callback handled.
            return Ok(new
            {
                transactionId = result.TransactionId,
                status = result.Status,
                statusDetail = result.StatusDetail,
                threeDSecure = result.ThreeDSecure?.Status
            });
        }
        catch (ElavonValidationException ex)
        {
            await _store.Mark3DsFailedAsync(pending.TransactionId, "Validation", ex.Message, ct);
            return BadRequest(new { error = "Validation", details = ex.Errors });
        }
        catch (ElavonApiException ex)
        {
            await _store.Mark3DsFailedAsync(pending.TransactionId, "ApiError", ex.RawResponse, ct);
            return StatusCode(ex.HttpStatusCode, new { error = ex.ErrorCode, raw = ex.RawResponse });
        }
    }
}
```

### 14.5 Minimal state store contract

```csharp
public sealed class CheckoutState
{
    public required string TransactionId { get; init; }
    public required string Status { get; set; }
    public string? StatusDetail { get; set; }
    public DateTimeOffset CreatedUtc { get; init; }
}

public interface ICheckoutStateStore
{
    Task SaveAsync(CheckoutState state, CancellationToken ct = default);
    Task<CheckoutState?> GetMostRecentPending3DsAsync(CancellationToken ct = default);
    Task Mark3DsCompletedAsync(string transactionId, string status, string? statusDetail, CancellationToken ct = default);
    Task Mark3DsFailedAsync(string transactionId, string failureType, string? detail, CancellationToken ct = default);
}
```

Production note:
- In real systems, do not rely on "most recent pending" lookup.
- Use an explicit correlation key per checkout and persist it through browser state, then resolve the exact transaction during callback handling.

### 14.6 Frontend Redirect Form + Callback Roundtrip

Your frontend usually needs to auto-submit the customer to ACS using `AcsUrl` and `CReq`, then your backend completes 3DS when the callback arrives.

#### Frontend: auto-submit to ACS via POST

```html
<!doctype html>
<html>
<body onload="document.getElementById('acsForm').submit()">
    <form id="acsForm" method="POST" action="{{acsUrl}}">
        <input type="hidden" name="creq" value="{{cReq}}" />
        <noscript>
            <p>JavaScript is disabled. Please click continue.</p>
            <button type="submit">Continue</button>
        </noscript>
    </form>
</body>
</html>
```

#### Backend: callback endpoint receives `cres`

```csharp
[HttpPost("/webhooks/opayo/3ds")]
[Consumes("application/x-www-form-urlencoded")]
public async Task<IActionResult> Receive([FromForm(Name = "cres")] string? cres, CancellationToken ct)
{
        if (string.IsNullOrWhiteSpace(cres))
                return BadRequest("Missing cres");

        var pending = await _store.GetMostRecentPending3DsAsync(ct);
        if (pending is null)
                return NotFound();

        var result = await _checkoutPaymentService.Complete3DsAsync(pending.TransactionId, cres, ct);
        return Ok(new { result.TransactionId, result.Status, result.StatusDetail });
}
```

#### End-to-end roundtrip summary

1. Backend starts transaction (`CreateTransactionAsync`).
2. If `Status == "3DAuth"`, backend returns `acsUrl` + `cReq` to frontend.
3. Frontend POSTs `creq` to ACS.
4. ACS authenticates customer and POSTs `cres` to your callback URL.
5. Backend calls `Complete3DsAsync(transactionId, new Complete3DsRequest { CRes = cres })`.
6. Backend marks checkout paid/failed and returns final status to frontend.

### 14.7 Common Integration Mistakes (and Fixes)

- Symptom: ACS URL returns 405 Method Not Allowed.
    Cause: ACS URL was opened with GET.
    Fix: Always POST form field `creq` to `acsUrl`.

- Symptom: 3DS completion returns validation/API errors after retrying simulator steps.
    Cause: Reused stale `cReq` or `acsTransactionID` from a previous/expired challenge.
    Fix: Start a fresh transaction and complete challenge once, end-to-end, with the new values.

- Symptom: Completion fails due to missing/invalid challenge response field.
    Cause: Wrong field naming between ACS callback and Opayo completion payload.
    Fix: Read callback field `cres` (lowercase) and pass it to SDK as `Complete3DsRequest.CRes` (capital R in property, serialized to `cRes`).

## 15. Where to Go Next

- See README.md for quick start and architectural overview.
- See RETRYING_AND_RELIABILITY.md for timeout/retry behavior details.
- See AI_TOOLING_PLAYGROUND.md for deterministic playground operation guidance.
