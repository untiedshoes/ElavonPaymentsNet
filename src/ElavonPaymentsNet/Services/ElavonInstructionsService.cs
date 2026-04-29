using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides instruction operations for managing transaction lifecycle states.
/// Access via <c>client.Instructions</c>.
/// </summary>
public class ElavonInstructionsService : IElavonInstructionsService
{
    private readonly ElavonApiClient _api;
    private readonly ElavonPaymentsClientOptions _options;

    internal ElavonInstructionsService(ElavonApiClient api, ElavonPaymentsClientOptions options)
    {
        _api = api;
        _options = options;
    }

    /// <summary>
    /// Posts an instruction to an existing transaction.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to instruct.</param>
    /// <param name="request">The instruction to apply, including type and optional amount.</param>
    public async Task<InstructionResponse> CreateInstructionAsync(
        string transactionId,
        InstructionRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<InstructionRequest, InstructionResponse>(
            HttpMethod.Post, $"/transactions/{transactionId}/instructions", request, null,
            _options.IntegrationKey, _options.IntegrationPassword, cancellationToken)
            .ConfigureAwait(false);
    }
}
