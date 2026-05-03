# AI Tooling Guide: Playground Operations

This document is for AI agents and automation tooling if you wish for them to operate the playground:

- Project: `playground/ElavonPaymentsNet.Playground`
- Entry point: `Program.cs`

Goal: run reliable, repeatable flows (especially 3DSv2) and avoid stale/invalid simulator states.

## 1. Core Rules for AI Tooling

1. Use a fresh transaction for every 3DS attempt.
2. Do not reuse old `cReq` values.
3. Do not reuse old `acsTransactionID` values.
4. Perform ACS steps immediately after receiving `cReq` (avoid timeout).
5. Treat simulator `Erro` payloads as terminal for that transaction.

## 2. Canonical Playground Launch

```bash
dotnet run --project playground/ElavonPaymentsNet.Playground/ElavonPaymentsNet.Playground.csproj
```

Interactive defaults:
- Card: `4929000000006`
- Expiry: `1229`
- CVV: `123`
- Cardholder: `Sandbox Tester`
- Magic cardholder for challenge simulation: `CHALLENGE`
- Apply3DSecure for deterministic challenge: `Force`

## 3. Prompt-by-Prompt Script (Deterministic Challenge)

Use this sequence:
1. Enter on card number prompt.
2. Enter on expiry prompt.
3. Enter on CVV prompt.
4. Enter on cardholder prompt.
5. Type `CHALLENGE` for magic cardholder.
6. Type `Force` for `Apply3DSecure`.

Expected transaction response state:
- `Status: 3DAuth`
- `StatusCode: 2021`
- `AcsUrl` printed
- `cReq` printed

## 4. ACS Simulator Interaction Rules

Important:
- `AcsUrl` must be called with HTTP POST.
- GET to `AcsUrl` returns 405 Method Not Allowed.
- POST field name is `creq`.
- Simulator response pages are stateful and can expire.

### Quick POST for first challenge page

```bash
curl -X POST --data-urlencode "creq=<cReq>" "https://sandbox.opayo.eu.elavon.com/3ds-simulator/html_challenge"
```

This returns HTML challenge content (not final `cres` yet).

## 5. Reliable cRes Extraction Script

Use one cookie jar and one `acsTransactionID` per flow.

```bash
CREQ='<fresh cReq from current run>'
JAR=/tmp/acs.jar
rm -f "$JAR"

curl -s -c "$JAR" -b "$JAR" \
  -X POST 'https://sandbox.opayo.eu.elavon.com/3ds-simulator/html_challenge' \
  --data-urlencode "creq=$CREQ" > /tmp/step1.html

ACS_TX=$(grep -o 'name="acsTransactionID" value="[^"]*"' /tmp/step1.html | head -n1 | sed 's/.*value="//;s/"$//')

for i in 1 2 3; do
  curl -s -c "$JAR" -b "$JAR" \
    -X POST 'https://sandbox.opayo.eu.elavon.com/3ds-simulator/html_challenge_answer' \
    --data-urlencode "acsTransactionID=$ACS_TX" \
    --data-urlencode 'challengeData=challenge' > "/tmp/step$((i+1)).html"

  CRES=$(sed -n 's/.*name="cres" value="\([^"]*\)".*/\1/p' "/tmp/step$((i+1)).html" | head -n1)
  if [ -n "$CRES" ]; then
    echo "$CRES"
    break
  fi
done
```

Then paste `CRES` into playground prompt:
- `Paste the cRes value here and press Enter:`

## 6. Known Failure Patterns and Meanings

### 405 at simulator URL
Cause:
- Browser/GET used instead of POST.
Fix:
- POST `creq` form field to `AcsUrl`.

### API error code 1029 with description "ACS has provided an Erro message"
Cause:
- `cRes` is an `Erro` payload, not successful `CRes`.
Fix:
- Start a fresh transaction and run challenge once.

### `errorCode 305` and detail `Transaction is already CREQ_RECEIVED` / `CHALLENGE_COMPLETE`
Cause:
- Duplicate submission or stale state.
Fix:
- Do not reuse `cReq`/`acsTransactionID`; restart flow.

### `errorCode 402` transaction timed out
Cause:
- Challenge not completed in time.
Fix:
- Regenerate transaction and complete immediately.

## 7. Success Criteria

A completed run should end with:

- `3DS completion result:`
- `Status: Ok`
- `StatusDetail: The Authorisation was Successful.`
- `3DSecure: Authenticated`

## 8. Current Playground Behavior Notes

Current code prints:
- warning that direct ACS URL open causes 405
- a copy/paste curl command for initial POST
- detailed 3DS completion output (Status, StatusDetail, TransactionId, AcsTransId, DsTransId, 3DSecure)

Current code also catches 1029 and prints a targeted explanation about stale/consumed challenge state.

## 9. Updating This File as Playground Evolves

Whenever playground prompts or behavior change, update:
1. Prompt sequence in Section 3.
2. Script in Section 5 if simulator interaction changes.
3. Failure pattern mappings in Section 6.
4. Success criteria in Section 7.

Minimum change discipline:
- Keep commands copy-paste runnable.
- Keep examples aligned with actual prompt labels in `Program.cs`.
- Add new failure signatures immediately when discovered.
