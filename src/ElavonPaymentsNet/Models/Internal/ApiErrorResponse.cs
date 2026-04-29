namespace ElavonPaymentsNet.Models.Internal;

/// <summary>
/// Represents the error payload returned by the Elavon API on failure.
/// </summary>
internal sealed class ApiErrorResponse
{
    public string? Code { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<ApiErrorDetail>? Errors { get; init; }
}

internal sealed class ApiErrorDetail
{
    public string? Property { get; init; }
    public string? ClientMessage { get; init; }
    public string? Message { get; init; }
}
