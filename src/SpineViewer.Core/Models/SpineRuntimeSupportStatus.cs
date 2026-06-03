namespace SpineViewer.Core.Models;

/// <summary>
/// Describes whether a runtime can load a resolved Spine asset set.
/// </summary>
public enum SpineRuntimeSupportStatus
{
    /// <summary>
    /// The runtime can load the asset set without known compatibility warnings.
    /// </summary>
    Supported,

    /// <summary>
    /// The runtime can attempt the load, but the user should be warned first.
    /// </summary>
    SupportedWithWarnings,

    /// <summary>
    /// The runtime cannot load the asset set.
    /// </summary>
    Unsupported,
}
