# Release Notes (Draft)

## Version

- Proposed: 1.0.0

## Highlights

- Added strict `VendorTxCode` safeguards for transaction creation paths.
- Added deterministic unknown-outcome reconciliation API:
  - `IElavonTransactionService.ReconcileUnknownCreateOutcomeAsync(...)`
- Added `GET /transactions/{id}/instructions` support in instructions service.
- Corrected card identifier security-code link authentication to Basic auth.
- Removed unsupported card identifier remove operation from public surface.
- Added typed transport exception mapping:
  - `ElavonTransportException` for network/timeout failures before HTTP response.
- Hardened guidance for custom `HttpClient` pipelines to avoid unsafe POST retries.
- Expanded tests around resilience, service safety, and guard behavior.

## Reliability / Idempotency Improvements

- POST retry policy remains disabled at SDK infrastructure layer.
- GET retry behavior remains conservative and transient-only.
- Unknown POST outcomes now have a first-class reconciliation path.
- Consumer docs now include a production-safe checkout template.

## Breaking / Behavioral Notes

- `VendorTxCode` now enforces strict runtime validation:
  - non-empty
  - max 40 chars
  - allowed characters: letters, digits, `-`, `_`, `.`
- Transport failures are now surfaced as `ElavonTransportException` (inherits `ElavonApiException`).

## Validation Summary

- Unit tests: passing (latest local pass)
- Build: passing (latest local pass)

## Upgrade Guidance

- If you pass custom `HttpClient` handlers, ensure no retry policy replays non-GET requests.
- Handle `ElavonTransportException` in the same unknown-outcome flow as `ElavonServerException` for POST calls.
- Ensure your `VendorTxCode` generation strategy matches the stricter validation rules.
