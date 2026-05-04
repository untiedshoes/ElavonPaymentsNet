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

- 1. SDK Services Overview
- 2. Wallets Service
- 3. Apple Pay Session Service
- 4. Card Identifiers Service
- 5. Transactions Service
- 6. Retrieve Transaction
- 7. 3D Secure Service
- 8. Instructions Service
- 9. Post-Payment Service
- 10. Tokens Service
- 11. Payment Methods Catalog
# Services and Payment Methods

This guide combines:
- all SDK service areas exposed by ElavonPaymentsClient
- all currently supported payment methods
- practical request examples for each major operation

References:
- Opayo API reference: https://developer.elavon.com/products/en-uk/opayo/v1/api-reference
- Additional reference: https://github.com/academe/opayo-pi/blob/master/README.md

---

## Contents

- 1. SDK Services Overview
- 2. Wallets Service
- 3. Apple Pay Session Service
- 4. Card Identifiers Service
- 5. Transactions Service
- 6. Retrieve Transaction
- 7. 3D Secure Service
- 8. Instructions Service
- 9. Post-Payment Service
- 10. Tokens Service
- 11. Payment Methods Catalog
- 12. Current Gaps vs API Surface

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

### Retrieve a transaction by id

This supports card, PayPal, Apple Pay, and Google Pay responses through the same call.

```csharp
var tx = await client.Transactions.RetrieveTransactionAsync("TRANSACTION_ID");
```

---

## 7. 3D Secure Service

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


