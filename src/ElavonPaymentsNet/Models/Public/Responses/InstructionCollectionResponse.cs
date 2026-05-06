namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>
/// The collection of instructions that have been applied to a transaction,
/// as returned by <c>GET /transactions/{id}/instructions</c>.
/// </summary>
public sealed class InstructionCollectionResponse
{
    /// <summary>The list of instructions applied to the transaction, in chronological order.</summary>
    public IReadOnlyList<InstructionResponse> Instructions { get; init; } = [];
}
