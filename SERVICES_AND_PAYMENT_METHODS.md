# Services and Payment Methods

This guide combines:
- All SDK service areas exposed by ElavonPaymentsClient
- All currently supported payment methods
- Practical request examples for each major operation

References:
- Opayo API reference: https://developer.elavon.com/products/en-uk/opayo/v1/api-reference
- Additional reference: https://github.com/academe/opayo-pi/blob/master/README.md

---

## Contents

1. SDK Services Overview
2. Wallets Service
3. Apple Pay Session Service
4. Card Identifiers Service
5. Transactions Service
6. Retrieve Transaction
7. 3D Secure Service
8. Instructions Service
9. Post-Payment Service
10. Tokens Service
11. Payment Methods Catalog
12. Handling Expired Session Keys and Tokens

---

## 1. SDK Services Overview

The client exposes these service groups:

- client.Wallets
- client.CardIdentifiers
- client.Transactions
- client.ThreeDs
- client.Instructions
- client.PostPayments
- client.Tokens

---

## 2. Wallets Service

The Wallets Service manages merchant session keys (MSK) and Apple Pay sessions. Session keys are temporary tokens that enable browser-based card identifier creation without exposing card details to your backend. They provide PCI compliance advantages by keeping raw card data client-side during the credential capture phase.

Use this service when building browser-based payment flows where you want to tokenize cards on the frontend, or when setting up Apple Pay payments through your merchant account. Always validate session keys before use—they have limited lifespans and may expire between requests.

References:
- [Opayo session key documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)
- [Tokenization flow examples](https://github.com/academe/opayo-pi/blob/master/README.md)

### Create a new merchant session key

Use this before browser-based card identifier flows.

```csharp
var msk = await client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest
{
    VendorName = "sandbox"
});
```

### Check merchant session key validity

```csharp
var validation = await client.Wallets.ValidateMerchantSessionKeyAsync(
    new MerchantSessionValidationRequest
    {
        MerchantSessionKey = msk.MerchantSessionKey
    });
```

---

## 3. Apple Pay Session Service

The Apple Pay Session Service handles the merchant validation handshake required for Apple Pay button integration. When a user taps an Apple Pay button on your site, Apple requires your merchant to validate its identity with Apple's servers before the payment flow proceeds. This session response is returned to the browser, which then continues the Apple Pay authorization flow.

Use this service in your backend endpoint that the frontend calls after Apple Pay button interaction. The service handles merchant validation certificate configuration (production vs. sandbox) and returns the session data needed by Apple's payment sheet.

References:
- [Opayo Apple Pay documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

### Create an Apple Pay session

```csharp
var applePaySession = await client.Wallets.CreateApplePaySessionAsync(new ApplePaySessionRequest
{
    VendorName = "sandbox",
    Domain = "merchant.example.com"
});
```

---

## 4. Card Identifiers Service

The Card Identifier (a temporary, tokenised card detail, where the card is actually stored at Opayo) can be created using the equally temporary merchant session key.

Normally it would be created on the front end, using an AJAX request from your browser, so the card details would never touch your application. For testing and development, the card details can be sent from your test script, emulating the front end, and that is detailed below.

Once created, a card identifier can be linked (verified with the security code), saved for recurring use, or removed when no longer needed. The identifier is used in subsequent transactions instead of raw card data.

References:
- [Card tokenization guide](https://github.com/academe/opayo-pi/blob/master/README.md)
- [Opayo card identifier documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

### Create a new card identifier

```csharp
var cardId = await client.CardIdentifiers.CreateCardIdentifierAsync(
    msk.MerchantSessionKey,
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

### Link a card identifier

```csharp
await client.CardIdentifiers.LinkCardIdentifierAsync(
    cardId.CardIdentifier!,
    new LinkCardIdentifierRequest
    {
        SecurityCode = "123"
    });
```

### Remove a card identifier

```csharp
await client.CardIdentifiers.RemoveCardIdentifierAsync(cardId.CardIdentifier!);
```

---

## 5. Transactions Service

The Transactions Service is the core API for all payment operations. It supports multiple transaction types, each designed for different payment scenarios:

- **Payment**: Immediate capture of funds (one-step payment)
- **Deferred**: Authorization without capture; funds are reserved but not captured until explicitly requested later via the Post-Payments service (two-step payment)
- **Authenticate**: Pure cardholder authentication without capturing funds; useful for verifying identity before sensitive operations
- **Repeat**: Subsequent payment using a previously authorized transaction ID, typically with stored/recurring credential metadata for subscription or installment flows
- **Refund**: Full or partial refund as a transaction type (alternative to using Post-Payments service)
- **Authorise**: Similar to Deferred, reserves funds for later capture

Each transaction type accepts payment method details (card identifier, Apple Pay, Google Pay, PayPal, or regional payment methods) and returns a transaction ID for reference.

References:
- [Opayo transaction types documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

Use CreateTransactionAsync with TransactionType to perform Payment, Deferred, Authenticate, Repeat, Refund, and Authorise.

### Payment (card identifier)

```csharp
var payment = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode = $"PAY-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Description = "Card payment",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier = cardId.CardIdentifier
        }
    },
    BillingAddress = new BillingAddress { Address1 = "88", City = "London", PostalCode = "412", Country = "GB" }
});
```

### Deferred

```csharp
var deferred = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Deferred,
    VendorTxCode = $"DEF-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Description = "Deferred payment",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier = cardId.CardIdentifier
        }
    },
    BillingAddress = new BillingAddress { Address1 = "88", City = "London", PostalCode = "412", Country = "GB" }
});
```

### Authenticate

```csharp
var authenticate = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Authenticate,
    VendorTxCode = $"AUTHN-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 100,
    Currency = "GBP",
    Description = "Authenticate cardholder",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier = cardId.CardIdentifier
        }
    },
    BillingAddress = new BillingAddress { Address1 = "88", City = "London", PostalCode = "412", Country = "GB" }
});
```

### Repeat

Requires RelatedTransactionId and usually MIT credential metadata.

```csharp
var repeat = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Repeat,
    VendorTxCode = $"REP-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    RelatedTransactionId = "PREVIOUS_TX_ID",
    CredentialType = new CredentialType
    {
        CofUsage = "Subsequent",
        InitiatedType = "MIT",
        MitType = "Unscheduled"
    }
});
```

### Refund (transaction-type refund)

```csharp
var refundTx = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Refund,
    VendorTxCode = $"RFD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 500,
    Currency = "GBP",
    RelatedTransactionId = "ORIGINAL_TX_ID",
    Description = "Partial refund"
});
```

### Authorise

```csharp
var authorise = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Authorise,
    VendorTxCode = $"AUTH-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Description = "Authorise then capture later",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier = cardId.CardIdentifier
        }
    },
    BillingAddress = new BillingAddress { Address1 = "88", City = "London", PostalCode = "412", Country = "GB" }
});
```

---

## 6. Retrieve Transaction

The Retrieve Transaction service allows you to fetch the full details of any previously created transaction by its ID. This is essential for reconciliation, status verification, fraud checks, and integration with your order management system.

Use this service to:
- Verify payment status after a timeout or connection loss
- Retrieve detailed response data for audit logging or compliance
- Check authorization/capture state for Deferred transactions
- Inspect 3D Secure challenge responses and decline reasons
- Support card, PayPal, Apple Pay, and Google Pay transactions through a single unified API

References:
- [Opayo retrieve transaction documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

### Retrieve a transaction by id

This supports card, PayPal, Apple Pay, and Google Pay responses through the same call.

```csharp
var tx = await client.Transactions.RetrieveTransactionAsync("TRANSACTION_ID");
```

---

## 7. 3D Secure Service

The 3D Secure (3DSv2) Service handles the completion of strong customer authentication (SCA) challenges. When a transaction requires additional verification, the API returns a 3DAuth status with challenge data. Your customer then authenticates with their bank's ACS (Access Control Server), which returns a challenge response (cRes). You must submit this response back to Opayo to finalize the transaction.

Use this service when:
- A payment or authentication transaction returns status `3DAuth`, indicating a challenge is required
- The customer has completed 3D Secure verification with their bank
- You need to fulfill PSD2/SCA regulatory requirements
- You're implementing embedded or redirect-based authentication flows

References:
- [Opayo 3D Secure documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

### Create a 3D Secure challenge response (3DSv2 completion)

After a payment returns status 3DAuth, send cRes to complete the challenge:

```csharp
var complete = await client.ThreeDs.Complete3DsAsync(
    "TRANSACTION_ID",
    new Complete3DsRequest
    {
        CRes = cResFromAcsPost
    });
```

---

## 8. Instructions Service

The Instructions Service allows you to modify or reverse the state of authorized transactions before or after capture. It supports four instruction types:

- **Void**: Completely reverses an authorization, releasing any reserved funds (typically used before capture)
- **Abort**: Cancels a Deferred or Authorise transaction that's still in pending state
- **Release**: Releases funds from an authorization without processing a full refund (used when you decide not to capture)
- **Cancel**: General cancellation for various transaction states

Use this service for order cancellations, customer request fulfillment, or fraud prevention before you've captured the full authorized amount. Note that once a transaction has been captured, use the Post-Payments service for refunds instead.

References:
- [Opayo instructions documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

### Create an instruction (void, abort, release, cancel)

```csharp
var instruction = await client.Instructions.CreateInstructionAsync(
    "TRANSACTION_ID",
    new InstructionRequest
    {
        InstructionType = InstructionType.Void
    });
```

---

## 9. Post-Payment Service

The Post-Payment Service handles all operations on transactions after they've been authorized or captured. It enables you to capture reserved funds, issue refunds (full or partial), and void captured payments.

Use this service for:
- **Capture**: Complete a Deferred or Authorise transaction, moving funds from authorization to capture (two-step payment completion)
- **Refund**: Return funds to the customer after capture (full or partial amounts; useful for returns, corrections, or customer satisfaction)
- **Void**: Reverse a captured transaction and return all funds to the customer

These operations are essential for handling customer returns, billing corrections, and transaction reversals after funds have been captured.

References:
- [Opayo post-payment operations documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

### Capture

```csharp
var capture = await client.PostPayments.CaptureTransactionAsync(
    "TRANSACTION_ID",
    new CapturePaymentRequest { Amount = 1000 });
```

### Refund

```csharp
var refund = await client.PostPayments.RefundTransactionAsync(
    "TRANSACTION_ID",
    new RefundPaymentRequest
    {
        Amount = 500,
        VendorTxCode = $"RFD-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
        Description = "Partial refund"
    });
```

### Void

```csharp
var voidResult = await client.PostPayments.VoidTransactionAsync("TRANSACTION_ID");
```

---

## 10. Tokens Service

The Tokens Service handles payment processing with previously stored card tokens. It supports two main operations:

- **PayWithToken**: Execute a payment using a stored token (card identifier) without requiring the customer to re-enter card details. This is the primary method for recurring billing, subscriptions, and one-click checkout experiences.
- **CreateToken**: Generate a reusable token from card details. Note: This endpoint's availability depends on your Opayo environment configuration; verify support with your merchant manager before relying on it in production.

For most use cases, you'll use the Card Identifiers Service to create and manage tokenized cards, then execute payments via PayWithToken. Tokens can be reused across multiple transactions with appropriate credential metadata (CIT/MIT flags for regulatory compliance).

References:
- [Opayo token payment documentation](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)
- [Token reuse and recurring payments guide](https://github.com/academe/opayo-pi/blob/master/README.md)

### Pay with a previously stored token

```csharp
var tokenPayment = await client.Tokens.PayWithTokenAsync(new PayWithTokenRequest
{
    VendorTxCode = $"TOK-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount = 1000,
    Currency = "GBP",
    Token = "STORED_TOKEN"
});
```

### Create token

The SDK includes CreateTokenAsync, but verify endpoint support in your environment before relying on it.

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

---

## 11. Payment Methods Catalog

Opayo supports a comprehensive range of payment methods to serve global merchants and diverse customer preferences. All payment methods are submitted as part of the CreateTransactionRequest.PaymentMethod property when creating a transaction.

Choose the payment method based on your target market, compliance requirements, and customer base. Digital wallets (Apple Pay, Google Pay) and regional methods (iDEAL, Alipay, WeChat Pay) offer improved conversion in their respective regions. Each method returns consistent transaction and response data, allowing unified handling in your application.

References:
- [Opayo supported payment methods](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)

All payment methods are submitted via CreateTransactionRequest.PaymentMethod.

- Card
  - Raw card: CardNumber, ExpiryDate, SecurityCode, CardholderName
  - Card identifier: MerchantSessionKey and CardIdentifier
  - Saved card token: PaymentMethod.Token
- Apple Pay: PaymentMethod.ApplePay
- Google Pay: PaymentMethod.GooglePay
- PayPal: PaymentMethod.PayPal
- iDEAL: PaymentMethod.Ideal
- Alipay: PaymentMethod.Alipay
- WeChat Pay: PaymentMethod.WechatPay
- EPS: PaymentMethod.Eps
- Trustly: PaymentMethod.Trustly

---

## 12. Handling Expired Session Keys and Tokens

### Expired merchant session keys

Merchant session keys are short-lived. Handle expiry by refreshing the key and recreating the card identifier.

Recommended flow:

- Validate MSK before use via `ValidateMerchantSessionKeyAsync`.
- If invalid (or payment/card-identifier call fails due to expired session), call `CreateMerchantSessionKeyAsync` again.
- Re-create the card identifier with the new MSK.
- Retry the transaction once using the new card identifier.

```csharp
var msk = await client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest { VendorName = "sandbox" });

var validation = await client.Wallets.ValidateMerchantSessionKeyAsync(
    new MerchantSessionValidationRequest { MerchantSessionKey = msk.MerchantSessionKey! });

if (!validation.Valid)
{
    msk = await client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest { VendorName = "sandbox" });
}
```

### Expired or invalid stored card tokens

Stored tokens/card identifiers can become unusable (expired card, replaced card, merchant cleanup, issuer decline).

Recommended flow:

- Attempt payment with `PaymentMethod.Token` (or your stored reusable `CardIdentifier`).
- On token/card-identifier-specific validation or decline errors, mark token as stale.
- Prompt customer for fresh card details and create/save a new reusable identifier.
- Retry once with the new token only after successful re-consent/authentication.

Implementation guidance:

- Treat stale token failures as non-transient business failures, not transport retries.
- Do not blindly retry the same token repeatedly.
- Keep idempotent `VendorTxCode`/order correlation on your side to avoid duplicate charges.



