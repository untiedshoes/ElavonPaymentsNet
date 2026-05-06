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

    // -------------------------------------------------------------------------
    // GetInstructionsAsync
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that GetInstructionsAsync issues GET /transactions/{id}/instructions with Basic auth.
    /// </summary>
    [Fact(DisplayName = "GetInstructions UsesGetAndExpectedRoute")]
    public async Task GetInstructions_UsesGetAndExpectedRoute()
    {
        HttpRequestMessage? captured = null;
        var service = ServiceTestHelpers.CreateInstructionsService(async request =>
        {
            captured = request;
            return ServiceTestHelpers.Json(HttpStatusCode.OK,
                "{\"instructions\":[{\"instructionType\":\"Void\",\"date\":\"2026-01-01T00:00:00Z\"}]}");
        });

        var response = await service.GetInstructionsAsync("tx-1");

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Get, captured!.Method);
        Assert.Equal("/transactions/tx-1/instructions", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Basic", captured.Headers.Authorization!.Scheme);
        Assert.Single(response.Instructions);
        Assert.Equal(InstructionType.Void, response.Instructions[0].InstructionType);
    }

    /// <summary>
    /// Verifies that GetInstructionsAsync returns an empty list when the API returns no instructions.
    /// </summary>
    [Fact(DisplayName = "GetInstructions EmptyCollection ReturnsEmptyList")]
    public async Task GetInstructions_EmptyCollection_ReturnsEmptyList()
    {
        var service = ServiceTestHelpers.CreateInstructionsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{\"instructions\":[]}")));

        var response = await service.GetInstructionsAsync("tx-1");

        Assert.NotNull(response);
        Assert.Empty(response.Instructions);
    }

    [Fact(DisplayName = "GetInstructions NullTransactionId ThrowsArgumentException")]
    public async Task GetInstructions_NullTransactionId_ThrowsArgumentException()
    {
        var service = ServiceTestHelpers.CreateInstructionsService(
            _ => Task.FromResult(ServiceTestHelpers.Json(HttpStatusCode.OK, "{}")));

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.GetInstructionsAsync(null!));
        Assert.Equal("transactionId", ex.ParamName);
    }
}
