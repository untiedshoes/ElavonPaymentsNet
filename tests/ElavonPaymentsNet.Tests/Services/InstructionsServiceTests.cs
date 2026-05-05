using System.Net;

namespace ElavonPaymentsNet.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ElavonPaymentsNet.Services.ElavonInstructionsService"/>.
/// </summary>
[Trait("Category", "Unit")]
public sealed class InstructionsServiceTests
{
    /// <summary>
    /// Verifies that instruction creation posts to /transactions/{id}/instructions using Basic auth.
    /// </summary>
    [Fact(DisplayName = "Create UsesExpectedRouteAndAuth")]
    public async Task Create_UsesExpectedRouteAndAuth()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateInstructionsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}");
        });

        var response = await service.CreateInstructionAsync("tx-1", new InstructionRequest { InstructionType = InstructionType.Void });

        Assert.Equal(InstructionType.Void, response.InstructionType);
        Assert.NotNull(captured);
        Assert.Equal("/transactions/tx-1/instructions", captured!.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
    }

    /// <summary>
    /// Verifies that transaction identifiers are URI-escaped before building instruction routes.
    /// </summary>
    [Fact(DisplayName = "Create EscapesTransactionIdInRoute")]
    public async Task Create_EscapesTransactionIdInRoute()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateInstructionsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}");
        });

        await service.CreateInstructionAsync("tx/with space", new InstructionRequest { InstructionType = InstructionType.Void });

        Assert.NotNull(captured);
        Assert.Equal("/transactions/tx%2Fwith%20space/instructions", captured!.RequestUri!.AbsolutePath);
    }

    [Fact(DisplayName = "Create NullTransactionId ThrowsArgumentException")]
    public async Task Create_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateInstructionsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateInstructionAsync(null!, new InstructionRequest { InstructionType = InstructionType.Void }));
        Assert.Equal("transactionId", ex.ParamName);
    }

    [Fact(DisplayName = "Create NullRequest ThrowsArgumentNullException")]
    public async Task Create_NullRequest_ThrowsArgumentNullException()
    {
        var service = ServiceTestHelpers.CreateInstructionsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateInstructionAsync("tx-123", null!));
    }
}
