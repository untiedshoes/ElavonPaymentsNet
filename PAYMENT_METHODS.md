# Payment Methods

This guide covers every payment method the SDK supports — card-based flows, digital wallets, and redirect-based alternative payment methods.

All payment methods are set on `CreateTransactionRequest.PaymentMethod` using the `PaymentMethod` model.

---

## Contents

- [Card](#1-card)
  - [Raw card (server-to-server)](#11-raw-card-server-to-server)
  - [Drop-in / hosted card identifier flow](#12-drop-in--hosted-card-identifier-flow)
  - [Saving a card for reuse](#13-saving-a-card-for-reuse)
  - [Paying with a saved card token](#14-paying-with-a-saved-card-token)
- [Apple Pay](#2-apple-pay)
- [Google Pay](#3-google-pay)
- [PayPal](#4-paypal)
- [iDEAL](#5-ideal)
- [Alipay](#6-alipay)
- [WeChat Pay](#7-wechat-pay)
- [EPS](#8-eps)
- [Trustly](#9-trustly)
- [Response metadata per payment method](#10-response-metadata-per-payment-method)

---

## 1. Card

### 1.1 Raw card (server-to-server)

Use this for server-to-server integrations where the card number never passes through a browser. Only do this if you are fully PCI DSS compliant.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,   // £10.00 in pence
    Currency        = "GBP",
    Description     = "Online order",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            CardNumber    = "4929000000006",
            ExpiryDate    = "1229",   // MMYY
            SecurityCode  = "123",
            CardholderName = "Sandbox Tester"
        }
    },
    BillingAddress = new BillingAddress
    {
        Address1   = "88",
        City       = "London",
        PostalCode = "412",
        Country    = "GB"
    }
});
```

### 1.2 Drop-in / hosted card identifier flow

The recommended approach for most integrations. Card details are captured on the client side using the Opayo drop-in UI or your own JS form. They never touch your server; only the temporary `cardIdentifier` does.

**Step 1 — create a merchant session key (backend):**

```csharp
var msk = await client.Wallets.CreateMerchantSessionKeyAsync(new MerchantSessionRequest
{
    VendorName = "sandbox"
});
// msk.MerchantSessionKey is valid for 400 seconds
```

**Step 2 — capture card details on the frontend** using the Opayo drop-in component or JS API, which returns a `cardIdentifier`.

**Step 3 — submit the payment (backend):**

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "Online order",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier     = cardIdentifierFromBrowser
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

### 1.3 Saving a card for reuse

Set `Save = true` on `CardDetails`, add a `CredentialType` indicating a first use, and **force 3D Secure** (cardholder must be present to save).

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "First payment, save card",
    PaymentMethod = new PaymentMethod
    {
        Card = new CardDetails
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CardIdentifier     = cardIdentifierFromBrowser,
            Save               = true
        }
    },
    Apply3DSecure = Apply3DSecureOption.Force,
    CredentialType = new CredentialType
    {
        CofUsage      = "First",
        InitiatedType = "CIT"
    },
    BillingAddress = new BillingAddress { /* ... */ },
    StrongCustomerAuthentication = new StrongCustomerAuthentication { /* ... */ }
});

// If status is Ok and payment response card is reusable, persist
// response.PaymentMethod.Card.CardIdentifier as your stored token.
```

The `cardIdentifier` from a successful `Save = true` response is your reusable token. Store it against the customer.

### 1.4 Paying with a saved card token

For subsequent MIT (merchant-initiated) payments, use the `Token` property on `PaymentMethod`. No MSK or browser session needed.

```csharp
var repeat = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"ORDER-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "Subscription renewal",
    PaymentMethod = new PaymentMethod
    {
        Token = storedCardIdentifier
    },
    CredentialType = new CredentialType
    {
        CofUsage      = "Subsequent",
        InitiatedType = "MIT",
        MitType       = "Unscheduled"
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

Alternatively you can use the dedicated token flow (`client.Tokens`) to create a standalone token without making a payment first:

```csharp
var token = await client.Tokens.CreateTokenAsync(new CreateTokenRequest
{
    Card = new CardDetails
    {
        CardNumber     = "4929000000006",
        ExpiryDate     = "1229",
        SecurityCode   = "123",
        CardholderName = "Sandbox Tester"
    }
});

var tokenPayment = await client.Tokens.PayWithTokenAsync(new PayWithTokenRequest
{
    VendorTxCode = $"TOK-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount       = 1000,
    Currency     = "GBP",
    Token        = token.Token!
});
```

---

## 2. Apple Pay

Apple Pay is handled in two stages: merchant session validation (server-to-server), followed by a standard payment submission with the encrypted wallet payload.

> **Note:** Apple Pay functionality is not currently available to all acquirers. Contact Opayo support to confirm it is enabled for your account.

For the full setup walkthrough — including domain registration, downloading the domain verification file, configuring your Opayo account, and the complete frontend integration — see the official guide: [Apple Pay (Opayo managed certificate)](https://developer.elavon.com/products/en-uk/opayo/v1/apple-pay-opayo-managed-certificate).

### Prerequisites

- Domain registered with Opayo and Apple
- `apple-developer-merchantid-domain-association` file served at `/.well-known/` on that domain (the file is available to download from the Opayo developer portal)
- Apple Pay enabled for your acquirer/vendor account

### Stage 1 — Validate the merchant session

When the browser fires the `onvalidatemerchant` event, your backend calls Opayo to validate the session. You must respond within 30 seconds.

```csharp
var applePaySession = await client.Wallets.CreateApplePaySessionAsync(new ApplePaySessionRequest
{
    VendorName = "sandbox",
    Domain     = "merchant.example.com"
});

// Return applePaySession.SessionValidationToken to your frontend.
// The frontend calls appleSession.completeMerchantValidation(sessionValidationToken).
```

The response also includes `MerchantSessionIdentifier`, `Nonce`, `MerchantIdentifier`, `DomainName`, `DisplayName`, `Signature`, and `EpochTimeStamp`/`ExpiresAt` for the session window.

### Stage 2 — Submit the payment

After the customer authorises on their device, the Apple Pay JS API returns a payment token. Your frontend sends this to your backend, which submits it as a `Payment` transaction:

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"APPLEPAY-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "Apple Pay payment",
    PaymentMethod = new PaymentMethod
    {
        ApplePay = new ApplePayPaymentMethod
        {
            PaymentData           = applePayTokenBase64,    // from Apple Pay JS API
            ClientIpAddress       = shopperIpAddress,
            MerchantSessionKey    = msk.MerchantSessionKey,
            SessionValidationToken = applePaySession.SessionValidationToken,
            DisplayName           = "Visa 1234",
            PaymentMethodType     = "Debit"
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Applepay` contains `LastFourDigits` and `ExpiryDate`.

---

## 3. Google Pay

Google Pay does not require a separate merchant session validation step. Your frontend integrates the Google Pay JS API, which returns an encrypted token.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"GPAY-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "Google Pay payment",
    PaymentMethod = new PaymentMethod
    {
        GooglePay = new GooglePayPaymentMethod
        {
            Payload           = googlePayTokenBase64,   // from Google Pay JS API
            ClientIpAddress   = shopperIpAddress,
            MerchantSessionKey = msk.MerchantSessionKey
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Googlepay` contains `LastFourDigits` and `ExpiryDate`.

---

## 4. PayPal

PayPal integration is a redirect-based flow. The shopper is sent to PayPal to authorise the payment, then redirected back to your `CallbackUrl`.

> Contact Opayo support to confirm PayPal is enabled for your vendor account before integrating.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"PAYPAL-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "PayPal payment",
    PaymentMethod = new PaymentMethod
    {
        PayPal = new PayPalPaymentMethod
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CallbackUrl        = "https://merchant.example.com/paypal/callback"
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Paypal` contains `OrderId`, `PayerId`, `CaptureId`.

---

## 5. iDEAL

iDEAL is a Netherlands bank redirect scheme. The shopper selects their bank and is redirected to complete payment.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"IDEAL-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "EUR",
    Description     = "iDEAL payment",
    PaymentMethod = new PaymentMethod
    {
        Ideal = new IdealPaymentMethod
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CallbackUrl        = "https://merchant.example.com/ideal/callback",
            LanguageCode       = "nl"
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Ideal` contains `PaymentInfo`.

---

## 6. Alipay

Alipay is a Chinese mobile and online payment platform. The shopper is redirected or shown a QR code depending on the `ShopperPlatform` hint.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"ALI-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "Alipay payment",
    PaymentMethod = new PaymentMethod
    {
        Alipay = new AlipayPaymentMethod
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CallbackUrl        = "https://merchant.example.com/alipay/callback",
            LanguageCode       = "zh",
            ShopperPlatform    = "mobile"   // or "web"
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Alipay` — no additional metadata returned.

---

## 7. WeChat Pay

WeChat Pay is a Chinese mobile payment platform with wide international acceptance.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"WECHAT-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "WeChat Pay payment",
    PaymentMethod = new PaymentMethod
    {
        WechatPay = new WechatPayPaymentMethod
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CallbackUrl        = "https://merchant.example.com/wechat/callback",
            LanguageCode       = "zh",
            Bic                = "SFRTD45"   // bank identifier code if required
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Wechatpay` contains `UserAmount` (amount in shopper's currency) and `UserCurrency`.

---

## 8. EPS

EPS (Electronic Payment Standard) is an Austrian bank transfer scheme.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"EPS-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "EUR",
    Description     = "EPS payment",
    PaymentMethod = new PaymentMethod
    {
        Eps = new EpsPaymentMethod
        {
            MerchantSessionKey = msk.MerchantSessionKey,
            CallbackUrl        = "https://merchant.example.com/eps/callback",
            LanguageCode       = "de",
            Bic                = "SFRTD45"   // shopper's bank BIC
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Eps` contains `PaymentInfo` and `Bic`.

---

## 9. Trustly

Trustly is an open-banking payment method covering most European bank accounts. The shopper selects their bank and authorises in their online banking.

```csharp
var response = await client.Transactions.CreateTransactionAsync(new CreateTransactionRequest
{
    TransactionType = TransactionType.Payment,
    VendorTxCode    = $"TRUSTLY-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
    Amount          = 1000,
    Currency        = "GBP",
    Description     = "Trustly payment",
    PaymentMethod = new PaymentMethod
    {
        Trustly = new TrustlyPaymentMethod
        {
            MerchantSessionKey     = msk.MerchantSessionKey,
            CallbackUrl            = "https://merchant.example.com/trustly/callback",
            LanguageCode           = "en",
            ClientIpAddress        = shopperIpAddress,
            BeneficiaryId          = "your-beneficiary-id",
            BeneficiaryName        = "My Store",
            BeneficiaryAddress     = "1 Example Street, London",
            BeneficiaryCountryCode = "GB"
        }
    },
    BillingAddress = new BillingAddress { /* ... */ }
});
```

**Response:** `response.PaymentMethod.Trustly` contains `PaymentInfo`.

---

## 10. Response metadata per payment method

The `PaymentResponse.PaymentMethod` property is populated based on the method used. Only one method will be non-null.

| Method      | Response property                | Fields                                          |
|-------------|----------------------------------|-------------------------------------------------|
| Card        | `PaymentMethod.Card`             | `CardType`, `LastFourDigits`, `ExpiryDate`, `CardIdentifier`, `Reusable` |
| Apple Pay   | `PaymentMethod.Applepay`         | `LastFourDigits`, `ExpiryDate`                  |
| Google Pay  | `PaymentMethod.Googlepay`        | `LastFourDigits`, `ExpiryDate`                  |
| PayPal      | `PaymentMethod.Paypal`           | `OrderId`, `PayerId`, `CaptureId`               |
| iDEAL       | `PaymentMethod.Ideal`            | `PaymentInfo`                                   |
| Alipay      | `PaymentMethod.Alipay`           | _(none)_                                        |
| WeChat Pay  | `PaymentMethod.Wechatpay`        | `UserAmount`, `UserCurrency`                    |
| EPS         | `PaymentMethod.Eps`              | `PaymentInfo`, `Bic`                            |
| Trustly     | `PaymentMethod.Trustly`          | `PaymentInfo`                                   |

### Checking the response method

```csharp
var method = response.PaymentMethod;

if (method?.Card is { } card)
{
    Console.WriteLine($"Card: {card.CardType} ****{card.LastFourDigits} exp {card.ExpiryDate}");

    if (card.Reusable == true)
        Console.WriteLine($"Saved token: {card.CardIdentifier}");
}
else if (method?.Applepay is { } ap)
{
    Console.WriteLine($"Apple Pay: ****{ap.LastFourDigits} exp {ap.ExpiryDate}");
}
else if (method?.Paypal is { } pp)
{
    Console.WriteLine($"PayPal orderId={pp.OrderId} captureId={pp.CaptureId}");
}
```

---

## See also

- [SDK_CONSUMER_GUIDE.md](SDK_CONSUMER_GUIDE.md) — full end-to-end integration guide including 3DS, error handling, and post-payment operations
- [RETRYING_AND_RELIABILITY.md](RETRYING_AND_RELIABILITY.md) — retry behaviour and transient fault guidance
- [Opayo API Reference](https://developer.elavon.com/products/en-uk/opayo/v1/api-reference)
