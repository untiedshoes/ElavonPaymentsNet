namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Instructions service against the real Elavon sandbox API.
/// Covers Void, Abort, and Release instructions on previously created transactions.
/// </summary>
[Trait("Category", "Integration")]
public sealed class InstructionsIntegrationTests
{
    /// <summary>Verifies that a Void instruction can be posted to a completed payment.</summary>
    [Fact(DisplayName = "CreateInstructionAsync Void ReturnsVoidInstruction")]
    public async Task CreateInstructionAsync_Void_ReturnsVoidInstruction()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "INSTRVOID");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Void
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Void, result.InstructionType);
    }

    /// <summary>Verifies that an Abort instruction can be posted to a deferred transaction.</summary>
    [Fact(DisplayName = "CreateInstructionAsync Abort OnDeferred ReturnsAbortInstruction")]
    public async Task CreateInstructionAsync_Abort_OnDeferred_ReturnsAbortInstruction()
    {
        var txId = await SandboxHelpers.GetDeferredTransactionIdAsync();
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Abort
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Abort, result.InstructionType);
    }

    /// <summary>Verifies that a Release instruction can be posted to a deferred transaction.</summary>
    [Fact(DisplayName = "CreateInstructionAsync Release OnDeferred ReturnsReleaseInstruction")]
    public async Task CreateInstructionAsync_Release_OnDeferred_ReturnsReleaseInstruction()
    {
        var txId = await SandboxHelpers.GetDeferredTransactionIdAsync();
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.Instructions.CreateInstructionAsync(txId, new InstructionRequest
        {
            InstructionType = InstructionType.Release,
            Amount          = 100
        });

        Assert.NotNull(result);
        Assert.Equal(InstructionType.Release, result.InstructionType);
    }
}
