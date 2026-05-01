using ElavonPaymentsNet.Http;

namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Hardcoded Elavon/Opayo sandbox credentials for use in integration tests.
/// These credentials are publicly available in the Opayo PI REST API documentation
/// and are safe to include in source control — they only ever work against the
/// non-production sandbox environment.
/// </summary>
internal static class SandboxCredentials
{
    // ----------------------------------------------------------------
    // Basic sandbox — no 3DS extra checks applied.
    // VendorName: "sandbox"
    // ----------------------------------------------------------------

    internal static ElavonPaymentsClientOptions Basic => new()
    {
        IntegrationKey      = "hJYxsw7HLbj40cB8udES8CDRFLhuJ8G54O6rDpUXvE6hYDrria",
        IntegrationPassword = "o2iHSrFybYMZpmWOQMuhsXP52V4fBtpuSDshrKDSWsBY1OiN6hwd9Kb12z4j5Us5u",
        Environment         = ElavonEnvironment.Sandbox
    };

    internal const string BasicVendorName = "sandbox";

    // ----------------------------------------------------------------
    // Extra Checks sandbox — 3DS extra checks enabled.
    // VendorName: "sandboxEC"
    // ----------------------------------------------------------------

    internal static ElavonPaymentsClientOptions ExtraChecks => new()
    {
        IntegrationKey      = "dq9w6WkkdD2y8k3t4olqu8H6a0vtt3IY7VEsGhAtacbCZ2b5Ud",
        IntegrationPassword = "hno3JTEwDHy7hJckU4WuxfeTrjD0N92pIaituQBw5Mtj7RG3V8zOdHCSPKwJ02wAV",
        Environment         = ElavonEnvironment.Sandbox
    };

    internal const string ExtraChecksVendorName = "sandboxEC";
}
