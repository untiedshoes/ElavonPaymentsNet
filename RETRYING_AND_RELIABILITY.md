# 🔁 Retry Behaviour & Reliability Model

This document defines how retry behaviour, failure handling, and reliability guarantees work in the **Elavon .NET SDK**.

The SDK is designed for **financial safety over automatic recovery**. When in doubt, it does less — not more.

---

## 1. Core Principle

> **A payment that may have succeeded must never be blindly retried.**

The fundamental problem in financial systems is not *how to retry* — it is *knowing whether the original operation executed*. A network timeout does not mean the server did nothing. It means **you don't know what the server did.**

The SDK enforces this distinction at the infrastructure level. It is not configurable.

---

## 2. Retry Architecture Overview

Retry logic lives in a single `DelegatingHandler` in the HTTP pipeline, which runs transparently beneath every service call:

```
HttpClient
  └── ElavonLoggingHandler        (request/response diagnostics)
        └── ElavonAuthenticationHandler  (Basic / Bearer auth injection)
              └── ElavonResilienceHandler      (retry policy — GET only)
                    └── HttpClientHandler      (outbound TCP)
```

The `ElavonResilienceHandler` uses **Polly v8** with an exponential backoff + jitter strategy.

**Default settings** (configurable via `ElavonPaymentsClientOptions`):

| Setting            | Default | Range  |
|--------------------|---------|--------|
| `MaxRetryAttempts` | `3`     | 1 – 10 |
| Base delay         | 1 s     | fixed  |
| Backoff            | Exponential with jitter | — |
| Approximate delays | ~1 s, ~2 s, ~4 s + jitter | — |

---

## 3. What IS Retried Automatically

Only **HTTP GET requests** are eligible for automatic retry.

| Trigger                    | Retried? | Reason                                              |
|----------------------------|----------|-----------------------------------------------------|
| `HttpRequestException`     | ✅ Yes   | Network failure, DNS error, connection reset        |
| `TaskCanceledException`    | ✅ Yes   | HTTP timeout (not caller-initiated cancellation)    |
| 5xx response (`>= 500`)    | ✅ Yes   | Transient server error; state not mutated on GET    |

GET requests in this SDK are **read-only** (e.g. fetching a transaction status). Retrying them cannot cause a duplicate financial operation.

---

## 4. What is NOT Retried

| Request type          | Retried? | Reason                                              |
|-----------------------|----------|-----------------------------------------------------|
| **Any POST request**  | ❌ Never | May have executed server-side before connection dropped |
| **Any PUT request**   | ❌ Never | Same unknown-state risk                             |
| 4xx client errors     | ❌ Never | A request problem will not resolve with repetition  |
| Caller-cancelled token | ❌ Never | Explicit cancellation is honoured immediately       |

This is enforced unconditionally in `ElavonResilienceHandler`:

```csharp
// POST, PUT, PATCH, DELETE — all financial mutating operations.
// Never retry: a server-side effect may have occurred before the connection dropped.
if (request.Method != HttpMethod.Get)
    return base.SendAsync(request, cancellationToken);
```

There is no option to enable POST retries. This is intentional.

---

## 5. Why POST is Never Retried

Every mutating API operation in this SDK — `CreateTransaction`, `CaptureTransaction`, `RefundTransaction`, `VoidTransaction`, `CreateInstruction` — is a POST. Each one has **real-world financial consequences**.

Consider what happens on a transient failure during a POST:

```
Client ──POST /transactions──► Server
                                  └── Server processes payment ✅
Client ◄── TCP connection drops ──
```

From the client's perspective: the request "failed" with a `HttpRequestException` or a 5xx.  
From the server's perspective: the payment **was processed**.

If the SDK automatically retried, the customer would be **charged twice**.

The Opayo/Elavon API does not provide a client-side idempotency key mechanism (unlike Stripe's `Idempotency-Key` header), so there is no safe way to make a POST retry idempotent. The only safe choice is to never retry.

---

## 6. The Unknown State Problem

When a POST fails with a network error, timeout, or 5xx response, you are in **unknown state**:

```
POST /transactions  →  timeout / 5xx / connection reset
                             ↓
              ┌──────────────┴──────────────┐
              │                             │
    Server did NOT execute         Server DID execute
    (safe to retry)                (retry = duplicate charge)
              │                             │
              └──────────────┬──────────────┘
                             │
                    YOU DO NOT KNOW WHICH
```

### Recommended Recovery Pattern

1. **Do not retry the POST** immediately.
2. **Query the transaction** by `VendorTxCode` (your unique reference) using a GET.
3. **Inspect the status** returned to determine what actually happened.
4. **Act on confirmed state**, not on the exception type.

```csharp
try
{
    var result = await client.Transactions.CreateTransactionAsync(request);
    // Success path — confirmed executed
}
catch (ElavonApiException ex) when (ex is ElavonServerException or { HttpStatusCode: 0 })
{
    // Unknown state — do NOT retry blindly
    // Query by VendorTxCode to determine actual outcome before acting
}
```

---

## 7. Idempotency Limitation

> **This SDK cannot provide idempotent POST retries.**

The Opayo/Elavon PI REST API does not support an `Idempotency-Key` request header. This means:

- There is no server-side deduplication mechanism for financial operations.
- Sending the same POST twice will create two separate transactions.
- The SDK cannot safely retry any POST, regardless of the failure type.

**Your `VendorTxCode` is your idempotency anchor.** It is a caller-supplied unique reference that the Elavon API stores against each transaction. Use it to reconcile unknown state:

```csharp
// Always generate a stable, unique VendorTxCode per payment attempt
request.VendorTxCode = $"ORDER-{orderId}-{attemptNumber}";
```

If you need to retry after unknown state, generate a **new** `VendorTxCode` for the new attempt, then reconcile both by querying the original reference first.

---

## 8. Application Responsibility

The SDK handles **infrastructure-level transience** (network blips on safe operations). Your application is responsible for everything above that:

| Responsibility                                     | SDK | Your Application |
|----------------------------------------------------|-----|-----------------|
| Retry GET on network failure                       | ✅  | —               |
| Never retry POST automatically                     | ✅  | —               |
| Reconcile unknown POST state via GET               | —   | ✅              |
| Generate unique `VendorTxCode` per attempt         | —   | ✅              |
| Implement business-level retry with new references | —   | ✅              |
| Honour `Retry-After` on 429 responses              | —   | ✅ (SDK exposes `ElavonRateLimitException.RetryAfter`) |
| Persist transaction references for audit           | —   | ✅              |

---

## 9. Safe Retry Decision Rules

```
Was the operation a GET?
  YES → SDK retried automatically. You receive the final result.
  NO  → SDK did NOT retry. Apply the following:

Was the exception a 4xx (ElavonValidationException, ElavonAuthenticationException, etc.)?
  YES → Do not retry. Fix the request.

Was the exception a 429 (ElavonRateLimitException)?
  YES → Wait for RetryAfter, then retry with the same request.

Was the exception a 402 (ElavonPaymentDeclinedException)?
  YES → Do not retry. The card was declined. Present to the user.

Was the exception a 5xx (ElavonServerException) or a network error?
  YES → Unknown state. Query by VendorTxCode before deciding to retry.
```

---

## 10. Failure Handling Model

### 10.1 Business Failure — Do NOT Retry

These are **definitive outcomes**. The server processed the request and returned a clear result.

| Exception                        | Status | Meaning                        | Action                         |
|----------------------------------|--------|--------------------------------|--------------------------------|
| `ElavonAuthenticationException`  | 401    | Invalid credentials            | Fix credentials                |
| `ElavonValidationException`      | 400    | Malformed request              | Fix the request                |
| `ElavonPaymentDeclinedException` | 402    | Card declined by issuer        | Present decline to user        |
| `ElavonRateLimitException`       | 429    | Too many requests              | Wait `RetryAfter`, then retry  |

### 10.2 Technical Failure — SDK Retries GET Only

These are **transient conditions** where the server may not have been reached at all, or may have failed before completing — but only on GET (read-only) requests.

| Trigger                    | SDK Behaviour          |
|----------------------------|------------------------|
| `HttpRequestException`     | Retry with backoff     |
| `TaskCanceledException`    | Retry with backoff     |
| 5xx on GET                 | Retry with backoff     |
| 5xx on POST                | **Propagate immediately — do not retry** |

### 10.3 Unknown State — The Most Important Case

A 5xx or network exception on a **POST** means the operation may or may not have executed. **This is the most dangerous state in financial processing.**

```
ElavonServerException on POST
  → Transaction status: unknown
  → Do NOT retry the POST
  → DO query by VendorTxCode
  → Act only on confirmed state
```

---

## 11. Recommended Production Flow

```
1. Generate a unique VendorTxCode for this payment attempt.
2. Call CreateTransactionAsync.
3a. SUCCESS  → Record the Elavon TransactionId. Done.
3b. 4xx      → Fix the request or present to user. Do not retry.
3c. 402      → Card declined. Present to user. Do not retry.
3d. 429      → Wait RetryAfter seconds. Retry the same request.
3e. 5xx / network error →
      4. Query transaction by VendorTxCode.
      5a. Found + Authorised  → Payment executed. Record. Do not charge again.
      5b. Found + Failed      → Payment failed cleanly. Safe to retry with new VendorTxCode.
      5c. Not found           → Payment did not execute. Safe to retry with new VendorTxCode.
```

---

## 12. Design Philosophy

The SDK makes the **conservative choice** at every decision point:

- **Polly** is used for GET-only retry with exponential backoff and jitter. Jitter prevents retry storms when multiple clients fail simultaneously.
- **POST is never retried** — not even once — regardless of failure type. No configuration option enables this.
- **4xx errors are never retried** — a malformed request will not fix itself.
- **Caller cancellation is never retried** — if your `CancellationToken` fires, the SDK stops immediately.
- **No idempotency key is fabricated** — without server-side support, a fabricated key provides false safety.

This follows the same reasoning used by major payment SDKs (e.g. Stripe .NET), which also never retry POST requests automatically.

- **Exception translation happens once, at the HTTP boundary** — `ElavonApiClient` catches all raw infrastructure exceptions (`HttpRequestException`, `JsonException`, non-success HTTP responses) and converts them into typed `ElavonApiException` derivatives before they reach any service method. By the time an exception surfaces to the caller it has already been classified and carries structured information (`HttpStatusCode`, `ErrorCode`, `RawResponse`). Service methods therefore contain no try-catch blocks — not because error handling was omitted, but because it was already done at the correct layer. Adding catch blocks in services would re-wrap already-typed exceptions, losing information and introducing a second translation layer where none is needed.

---

## 13. Mental Model for Developers

```
GET  → Read. Safe to retry. SDK handles this.
POST → Write. May have executed. YOU handle the consequence.
```

More specifically:

```
Exception thrown on GET
  → "The read failed. The SDK retried it N times. It still failed."
  → Safe to propagate and surface as a transient error.

Exception thrown on POST
  → "A write was attempted. It may have executed."
  → Query by VendorTxCode to determine actual state before acting.
```

Think of it this way: **an exception from a POST is not a failure report — it is a question mark.**

---

## 14. Anti-Patterns

**Do NOT:**

```csharp
// ❌ Blindly retry a POST
for (var i = 0; i < 3; i++)
{
    try { await client.Transactions.CreateTransactionAsync(request); break; }
    catch { continue; } // may charge the customer 3 times
}

// ❌ Assume a timeout means the payment did not happen
catch (TaskCanceledException)
{
    return PaymentResult.Failed; // wrong — payment may have succeeded
}

// ❌ Skip reconciliation after unknown state
catch (ElavonServerException)
{
    await RetryPaymentAsync(request); // dangerous without querying first
}

// ❌ Treat the exception type as definitive state for POST
catch (ElavonApiException ex) when (ex.HttpStatusCode >= 500)
{
    order.Status = OrderStatus.PaymentFailed; // may be incorrect
}
```

**Do:**

```csharp
// ✅ Query to confirm state before deciding to retry
catch (ElavonServerException)
{
    var txStatus = await QueryByVendorTxCodeAsync(request.VendorTxCode);
    if (txStatus is null || txStatus.Status == "Failed")
    {
        // Safe to retry with a new VendorTxCode
        request.VendorTxCode = GenerateNewReference();
        await client.Transactions.CreateTransactionAsync(request);
    }
    // else: payment succeeded — record it, do not charge again
}

// ✅ Honour Retry-After on 429
catch (ElavonRateLimitException ex)
{
    if (ex.RetryAfter.HasValue)
        await Task.Delay(ex.RetryAfter.Value, cancellationToken);
    // then retry the same request — rate limit errors are safe to retry
}
```

---

## 15. Summary

| Question                                          | Answer                                              |
|---------------------------------------------------|-----------------------------------------------------|
| Does the SDK retry failed requests?               | Only GET requests, automatically                    |
| Are POST (payment) requests ever retried?         | Never — by design                                   |
| Why not?                                          | The server may have executed before the error       |
| Does the Elavon API support idempotency keys?     | No                                                  |
| What should I do after a POST 5xx?                | Query by `VendorTxCode`, then act on confirmed state |
| What should I do after a POST 4xx?                | Fix the request. Do not retry.                      |
| What should I do after a 429?                     | Wait `ElavonRateLimitException.RetryAfter`, then retry |
| Can I increase the number of retries?             | Yes — via `MaxRetryAttempts` (1–10, GET only)       |
| Can I enable POST retries?                        | No                                                  |
