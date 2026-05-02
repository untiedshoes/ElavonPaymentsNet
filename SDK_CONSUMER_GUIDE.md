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

### 4.5 Retrieve a Transaction

```csharp
var tx = await client.Transactions.RetrieveTransactionAsync("<transaction-id>");
Console.WriteLine($"Status={tx.Status} Code={tx.StatusCode}");
```

### Note on Authenticate Transaction Type

The upstream API supports an Authenticate type, but the current public `TransactionType` enum in this SDK includes:
- Payment
- Authorise
- Deferred
- Repeat

If Authenticate is required in your project, add it deliberately as a surfaced SDK feature with tests, schema updates, and docs updates.

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
- `Void`
- `Abort`
- `Release`
- `Cancel`

## 8. Merchant Session Key and Card Identifiers

### Create merchant session key

```csharp
var msk = await client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest
{
    VendorName = "sandbox"
});
```

### Validate merchant session key

```csharp
var check = await client.Wallets.ValidateMerchantSessionKeyAsync(new MerchantSessionValidationRequest
{
    MerchantSessionKey = msk.MerchantSessionKey!
});
```

### Create card identifier

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
```

### Optional: Link CVV after identifier creation

```csharp
await client.CardIdentifiers.LinkCardIdentifierAsync(
    cardIdentifier.CardIdentifier!,
    new LinkCardIdentifierRequest { SecurityCode = "123" });
```

### Pay using card identifier

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

```csharp
var applePaySession = await client.Wallets.CreateApplePaySessionAsync(new ApplePaySessionRequest
{
    ValidationUrl = "https://apple-pay-gateway.apple.com/paymentservices/startSession",
    DomainName = "merchant.example.com"
});
```

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

## 14. Where to Go Next

- See README.md for quick start and architectural overview.
- See RETRYING_AND_RELIABILITY.md for timeout/retry behavior details.
- See AI_TOOLING_PLAYGROUND.md for deterministic playground operation guidance.
