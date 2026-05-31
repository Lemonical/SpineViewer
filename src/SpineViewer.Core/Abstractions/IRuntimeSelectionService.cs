using SpineViewer.Core.Services;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Chooses the best runtime for a resolved Spine asset set.
/// </summary>
public interface IRuntimeSelectionService
{
    /// <summary>
    /// Selects the best available runtime and reports any compatibility warnings.
    /// </summary>
    /// <param name="request">The runtime-selection input.</param>
    /// <param name="cancellationToken">A token that cancels the selection operation.</param>
    /// <returns>The runtime-selection result.</returns>
    Task<RuntimeSelectionResult> SelectAsync(
        RuntimeSelectionRequest request,
        CancellationToken cancellationToken);
}
