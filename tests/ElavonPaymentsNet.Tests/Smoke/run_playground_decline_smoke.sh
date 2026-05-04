#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$repo_root"

output="$({ printf '4929602110085639\n\n\n\n\n\n1\n' | dotnet run --project playground/ElavonPaymentsNet.Playground --no-build; } 2>&1)"

echo "$output"

if [[ "$output" != *"Status:        NotAuthed"* ]]; then
  echo "Expected decline status NotAuthed was not found in playground output." >&2
  exit 1
fi

if [[ "$output" != *"The Authorisation was Declined by the bank."* ]]; then
  echo "Expected decline detail text was not found in playground output." >&2
  exit 1
fi

echo "Playground decline smoke check passed."
