namespace ElavonPaymentsNet.Validation;

/// <summary>
/// Provides shared argument validation helpers for infrastructure services.
/// </summary>
internal static class Guard
{
    private const int MaxVendorTxCodeLength = 40;

    /// <summary>
    /// Ensures the provided identifier is a positive integer.
    /// </summary>
    internal static void PositiveId(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, "Identifier must be a positive integer.");
    }

    /// <summary>
    /// Ensures the provided string contains a non-empty value.
    /// </summary>
    internal static void NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
    }

    /// <summary>
    /// Ensures the provided vendor transaction code is non-empty and safe.
    /// Allowed characters: letters, digits, hyphen, underscore, and dot.
    /// Maximum length: 40 characters.
    /// </summary>
    internal static void VendorTxCode(string? value, string paramName)
    {
        NotNullOrWhiteSpace(value, paramName);

        if (value!.Length > MaxVendorTxCodeLength)
            throw new ArgumentException($"Value cannot exceed {MaxVendorTxCodeLength} characters.", paramName);

        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_' or '.')
                continue;

            throw new ArgumentException(
                "Value contains invalid characters. Allowed: letters, digits, hyphen (-), underscore (_), dot (.).",
                paramName);
        }
    }
}