namespace ElavonPaymentsNet.Models.Public.Responses;

/// <summary>Response returned after posting an instruction to a transaction.</summary>
public sealed class InstructionResponse
{
    /// <summary>The instruction type that was applied.</summary>
    public InstructionType InstructionType { get; init; }

    /// <summary>The date and time the instruction was processed.</summary>
    public DateTime Date { get; init; }
}
