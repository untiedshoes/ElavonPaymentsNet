namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// The instruction to apply to an existing transaction via the Instructions endpoint.
/// </summary>
public enum InstructionType
{
    /// <summary>Voids a payment before settlement (same-day cancellation).</summary>
    Void,

    /// <summary>Aborts an in-progress 3D Secure challenge.</summary>
    Abort,

    /// <summary>Releases a deferred payment without capturing funds.</summary>
    Release,

    /// <summary>Cancels a pending or authorised transaction.</summary>
    Cancel
}
