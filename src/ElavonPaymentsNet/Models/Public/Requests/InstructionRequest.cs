namespace ElavonPaymentsNet.Models.Public.Requests;

/// <summary>
/// Request model for sending an instruction to an existing transaction.
/// </summary>
public sealed class InstructionRequest
{
    /// <summary>The instruction to apply to the transaction.</summary>
    public required InstructionType InstructionType { get; init; }

    /// <summary>
    /// Amount in the smallest currency unit. Required for partial-amount instructions
    /// where applicable; omit to apply the instruction to the full transaction amount.
    /// </summary>
    public int? Amount { get; init; }
}
