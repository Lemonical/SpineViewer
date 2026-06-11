using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Represents one progress update emitted during a Spine project open/load workflow.
/// </summary>
public sealed record SpineLoadProgress
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineLoadProgress"/> class.
    /// </summary>
    /// <param name="stage">The current workflow stage.</param>
    /// <param name="message">The user-facing progress message.</param>
    /// <param name="percentComplete">The optional completion ratio from <c>0.0</c> to <c>1.0</c>.</param>
    public SpineLoadProgress(
        SpineLoadStage stage,
        string message,
        double? percentComplete = null)
    {
        Stage = stage;
        Message = Guard.NotNullOrWhiteSpace(message, nameof(message));

        if (percentComplete is not null &&
            (double.IsNaN(percentComplete.Value) ||
             double.IsInfinity(percentComplete.Value) ||
             percentComplete.Value < 0.0 ||
             percentComplete.Value > 1.0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentComplete),
                "Percent complete must be between 0.0 and 1.0.");
        }

        PercentComplete = percentComplete;
    }

    /// <summary>
    /// Gets the current workflow stage.
    /// </summary>
    public SpineLoadStage Stage { get; init; }

    /// <summary>
    /// Gets the user-facing progress message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the optional completion ratio from <c>0.0</c> to <c>1.0</c>.
    /// </summary>
    public double? PercentComplete { get; init; }
}
