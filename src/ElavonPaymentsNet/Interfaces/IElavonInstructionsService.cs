using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines instruction operations for managing transaction lifecycle states.</summary>
public interface IElavonInstructionsService
{
    /// <summary>
    /// Posts an instruction to an existing transaction.
    /// Use <see cref="ElavonPaymentsNet.Models.Public.InstructionType"/> to specify the operation:
    /// <c>Void</c>, <c>Abort</c>, <c>Release</c>, or <c>Cancel</c>.
    /// </summary>
    /// <param name="transactionId">The Elavon transaction ID to instruct.</param>
    /// <param name="request">The instruction to apply, including type and optional amount.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<InstructionResponse> CreateInstructionAsync(string transactionId, InstructionRequest request, CancellationToken cancellationToken = default);
}
