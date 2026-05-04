using System;

namespace ElavonPaymentsNet.Models.Public;

internal static class ThreeDsIndicatorMapper
{
    internal static TEnum? Parse<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<TEnum>(value, true, out var parsed)
            ? parsed
            : null;
    }

    internal static string? ToApi<TEnum>(TEnum? value) where TEnum : struct, Enum =>
        value?.ToString();
}
