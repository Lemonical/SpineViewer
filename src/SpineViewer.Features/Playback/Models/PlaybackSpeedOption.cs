namespace SpineViewer.Features.Playback.Models;

/// <summary>
/// Represents one selectable playback speed option for the transport UI.
/// </summary>
public sealed record PlaybackSpeedOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackSpeedOption"/> class.
    /// </summary>
    /// <param name="multiplier">The playback speed multiplier.</param>
    /// <param name="displayName">The user-facing speed label.</param>
    public PlaybackSpeedOption(double multiplier, string displayName)
    {
        if (double.IsNaN(multiplier) || double.IsInfinity(multiplier) || multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be a positive finite number.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be null or whitespace.", nameof(displayName));
        }

        Multiplier = multiplier;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the playback speed multiplier.
    /// </summary>
    public double Multiplier { get; init; }

    /// <summary>
    /// Gets the user-facing speed label.
    /// </summary>
    public string DisplayName { get; init; }
}
