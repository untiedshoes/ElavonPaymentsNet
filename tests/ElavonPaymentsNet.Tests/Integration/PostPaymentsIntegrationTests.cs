namespace ElavonPaymentsNet.Tests.Integration;

/// <summary>
/// Integration tests for the Post-Payments service against the real Elavon sandbox API.
/// Covers void, refund, and capture operations on previously created transactions.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PostPaymentsIntegrationTests
{
    /// <summary>Verifies that a transaction can be voided via PostPayments.</summary>
    [Fact(DisplayName = "VoidTransactionAsync AfterPayment ReturnsSuccess")]
    public async Task VoidTransactionAsync_AfterPayment_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "VOID");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.VoidTransactionAsync(txId);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("InstructionAccepted", result.Status);
        Assert.Contains("Void instruction accepted", result.StatusDetail);
    }

    /// <summary>Verifies that a full refund can be issued via PostPayments.</summary>
    [Fact(DisplayName = "RefundTransactionAsync FullAmount AfterPayment ReturnsSuccess")]
    public async Task RefundTransactionAsync_FullAmount_AfterPayment_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "REFUND");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.RefundTransactionAsync(txId, new RefundPaymentRequest
        {
            Amount       = 100,
            VendorTxCode = $"INTEGRATION-RFND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Description  = "Integration test full refund"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>Verifies that a partial refund can be issued via PostPayments.</summary>
    [Fact(DisplayName = "RefundTransactionAsync PartialAmount AfterPayment ReturnsSuccess")]
    public async Task RefundTransactionAsync_PartialAmount_AfterPayment_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetSuccessfulTransactionIdAsync(tag: "PARTIALRFND");
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.RefundTransactionAsync(txId, new RefundPaymentRequest
        {
            Amount       = 50,
            VendorTxCode = $"INTEGRATION-PARTIALRFND-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Description  = "Integration test partial refund"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
    }

    /// <summary>Verifies that a deferred transaction can be captured via PostPayments.</summary>
    [Fact(DisplayName = "CaptureTransactionAsync AfterDeferred ReturnsSuccess")]
    public async Task CaptureTransactionAsync_AfterDeferred_ReturnsSuccess()
    {
        var txId = await SandboxHelpers.GetDeferredTransactionIdAsync();
        if (txId is null) return;

        var client = new ElavonPaymentsClient(SandboxCredentials.Basic);
        var result  = await client.PostPayments.CaptureTransactionAsync(txId, new CapturePaymentRequest
        {
            Amount = 100
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        Assert.Equal("InstructionAccepted", result.Status);
        Assert.Contains("Release instruction accepted", result.StatusDetail);
    }
}
