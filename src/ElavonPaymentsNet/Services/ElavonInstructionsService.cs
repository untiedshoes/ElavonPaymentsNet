using ElavonPaymentsNet.Http;
using ElavonPaymentsNet.Interfaces;
using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Services;

/// <summary>
/// Provides instruction operations for managing transaction lifecycle states.
/// Access via <c>client.Instructions</c>.
/// </summary>
internal sealed class ElavonInstructionsService : IElavonInstructionsService
{
    private readonly ElavonApiClient _api;

    internal ElavonInstructionsService(ElavonApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Posts an instruction to an existing transaction.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to instruct.</param>
    /// <param name="request">The instruction to apply, including type and optional amount.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<InstructionResponse> CreateInstructionAsync(string transactionId, InstructionRequest request, CancellationToken cancellationToken = default)
    {
        return await _api.SendAsync<InstructionRequest, InstructionResponse>(
            HttpMethod.Post, ElavonApiRoutes.TransactionInstructions(transactionId), request, null, cancellationToken)
            .ConfigureAwait(false);
    }
}
