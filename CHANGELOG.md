# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Changed

- `Complete3DsResponse.StatusCode` now uses numeric typing (`int?`) with string-compatible deserialization via `JsonNumberHandling.AllowReadingFromString`.
- `MerchantRiskIndicator` and `StrongCustomerAuthentication` now reject conflicting dual-alias assignments (raw + typed) with `ArgumentException`.
- `CaptureTransactionAsync` and `VoidTransactionAsync` now return `PostPaymentResponse.Status = "InstructionAccepted"` (instead of synthetic `"Ok"`) and include an explanatory `StatusDetail` timestamped from the instruction response.

### Added

- Unit coverage for alias conflict behavior and status-code parsing in `ModelAliasAndStatusCodeTests`.
- Updated service/integration test assertions for the `InstructionAccepted` post-payment status contract.

---

## [1.0.0] - 2026-05-06

### Added

- `IElavonTransactionService.ResolveUnknownTransactionAsync` — deterministic resolution path for unknown POST outcomes (e.g. after a transport failure on `CreateTransactionAsync`).
- `ElavonTransportException` — typed exception for network/timeout failures that occur before an HTTP response is received. Inherits `ElavonApiException`; exposes the originating `TransportException` property.
- `IElavonInstructionsService.GetInstructionsAsync` — retrieves the instruction history for a transaction via `GET /transactions/{id}/instructions`.
- `InstructionCollectionResponse` response model.
- `Guard.VendorTxCode` — strict runtime validation (non-empty, max 40 characters, allowed: letters, digits, `-`, `_`, `.`).
- Production-safe checkout template in `SDK_CONSUMER_GUIDE.md` covering high-entropy `VendorTxCode` generation, idempotency, and unknown-outcome reconciliation.
- `Directory.Build.props` centralising shared MSBuild properties across all projects.
- `CHANGELOG.md` following Keep a Changelog conventions.

### Changed

- `CreateTransactionAsync`, `PayWithTokenAsync`, and `RefundTransactionAsync` now enforce `Guard.VendorTxCode` at the call site before any HTTP request is made.
- All three `ElavonApiClient` send paths now catch `HttpRequestException` and non-caller-cancelled `OperationCanceledException` and surface them as `ElavonTransportException` instead of letting raw .NET exceptions escape.
- `ElavonPaymentsClient` custom `HttpClient` overload now carries an XML doc `<remarks>` warning about attaching retry handlers that replay non-GET requests.

### Fixed

- Card identifier link call was incorrectly using Bearer auth; corrected to Basic auth.

### Removed

- `RemoveCardIdentifierAsync` — operation is not supported by the Opayo PI API; removed from the public surface entirely.

---

## Versioning Policy

This SDK follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html) independently of the underlying Opayo PI API version.

| Change | Bump |
|--------|------|
| Bug fix, no behaviour change | PATCH |
| New method, new optional property, new response field | MINOR |
| Removed/renamed public type, changed method signature | MAJOR |
| Opayo ships `/api/v2` and the SDK integrates it | MAJOR |

When a new MAJOR is released, the previous MAJOR enters maintenance mode (critical and security fixes only).
