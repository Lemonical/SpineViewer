using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one Spine runtime option that the viewer can select.
/// </summary>
public sealed record SpineRuntimeDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimeDescriptor"/> class.
    /// </summary>
    /// <param name="runtimeId">The stable identifier for the runtime.</param>
    /// <param name="displayName">The user-facing runtime label.</param>
    /// <param name="supportedExportRange">The supported Spine export range or family.</param>
    /// <param name="capabilities">The capabilities exposed by the runtime.</param>
    public SpineRuntimeDescriptor(
        string runtimeId,
        string displayName,
        string supportedExportRange,
        SpineRuntimeCapabilities capabilities)
    {
        RuntimeId = Guard.NotNullOrWhiteSpace(runtimeId, nameof(runtimeId));
        DisplayName = Guard.NotNullOrWhiteSpace(displayName, nameof(displayName));
        SupportedExportRange = Guard.NotNullOrWhiteSpace(supportedExportRange, nameof(supportedExportRange));
        Capabilities = Guard.NotNull(capabilities, nameof(capabilities));
    }

    /// <summary>
    /// Gets the stable identifier for the runtime.
    /// </summary>
    public string RuntimeId { get; init; }

    /// <summary>
    /// Gets the user-facing runtime label.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the supported Spine export range or family.
    /// </summary>
    public string SupportedExportRange { get; init; }

    /// <summary>
    /// Gets the capabilities exposed by the runtime.
    /// </summary>
    public SpineRuntimeCapabilities Capabilities { get; init; }
}
