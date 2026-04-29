using ElavonPaymentsNet.Models.Public.Requests;
using ElavonPaymentsNet.Models.Public.Responses;

namespace ElavonPaymentsNet.Interfaces;

/// <summary>Defines instruction operations for managing transaction lifecycle states.</summary>
public interface IElavonInstructionsService
{
    /// <summary>
    /// Posts an instruction to an existing transaction.
    /// Use <see cref="InstructionType"/> to specify the operation:
    /// <c>Void</c>, <c>Abort</c>, <c>Release</c>, or <c>Cancel</c>.
    /// </summary>
    Task<InstructionResponse> CreateInstructionAsync(string transactionId, InstructionRequest request, CancellationToken cancellationToken = default);
}
