namespace SpineViewer.Features.Shell.Models;

/// <summary>
/// Represents one runtime-selection option shown in the shell.
/// </summary>
public sealed record RuntimeSelectionOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeSelectionOption"/> class.
    /// </summary>
    /// <param name="runtimeId">The runtime identifier represented by the option, or <see langword="null" /> for auto-detect.</param>
    /// <param name="displayName">The user-facing option label.</param>
    /// <param name="description">The helper description shown beside the option.</param>
    public RuntimeSelectionOption(
        string? runtimeId,
        string displayName,
        string description)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(description));
        }

        RuntimeId = string.IsNullOrWhiteSpace(runtimeId) ? null : runtimeId;
        DisplayName = displayName;
        Description = description;
    }

    /// <summary>
    /// Gets the runtime identifier represented by the option, or <see langword="null" /> for auto-detect.
    /// </summary>
    public string? RuntimeId { get; init; }

    /// <summary>
    /// Gets the user-facing option label.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the helper description shown beside the option.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the option uses automatic runtime detection.
    /// </summary>
    public bool IsAutoDetect => RuntimeId is null;
}
