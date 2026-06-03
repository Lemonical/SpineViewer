using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one Spine feature that the selected runtime cannot fully support.
/// </summary>
public sealed record UnsupportedSpineFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedSpineFeature"/> class.
    /// </summary>
    /// <param name="code">The stable feature code.</param>
    /// <param name="displayName">The user-facing feature label.</param>
    /// <param name="details">Optional technical details about the limitation.</param>
    public UnsupportedSpineFeature(
        string code,
        string displayName,
        string? details = null)
    {
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code));
        DisplayName = Guard.NotNullOrWhiteSpace(displayName, nameof(displayName));
        Details = Guard.NullIfWhiteSpace(details);
    }

    /// <summary>
    /// Gets the stable feature code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the user-facing feature label.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets optional technical details about the limitation.
    /// </summary>
    public string? Details { get; init; }
}
