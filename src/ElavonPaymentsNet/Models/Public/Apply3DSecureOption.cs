namespace ElavonPaymentsNet.Models.Public;

/// <summary>
/// Controls whether 3D Secure authentication is applied to a transaction.
/// </summary>
public enum Apply3DSecureOption
{
    /// <summary>Use the account's MySagePay/MyOpayo 3DS settings (default behaviour).</summary>
    UseMSPSetting,

    /// <summary>Force 3DS authentication even if it is turned off at the account level.</summary>
    Force,

    /// <summary>Disable 3DS authentication and rules for this transaction.</summary>
    Disable,

    /// <summary>Force 3DS authentication but ignore rules.</summary>
    ForceIgnoringRules
}
