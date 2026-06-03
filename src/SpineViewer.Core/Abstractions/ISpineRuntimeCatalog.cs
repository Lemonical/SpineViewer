using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Exposes the runtime adapters available to the application.
/// </summary>
public interface ISpineRuntimeCatalog
{
    /// <summary>
    /// Gets all runtime descriptors known to the application.
    /// </summary>
    /// <returns>The registered runtime descriptors.</returns>
    IReadOnlyList<SpineRuntimeDescriptor> GetAvailableRuntimes();

    /// <summary>
    /// Gets the runtime adapter for a required runtime identifier.
    /// </summary>
    /// <param name="runtimeId">The runtime identifier to resolve.</param>
    /// <returns>The matching runtime adapter.</returns>
    ISpineRuntimeAdapter GetRequiredAdapter(string runtimeId);

    /// <summary>
    /// Attempts to get the best adapter suggested by version detection.
    /// </summary>
    /// <param name="versionMatch">The version-detection result to evaluate.</param>
    /// <param name="adapter">When this method returns <see langword="true"/>, the suggested adapter.</param>
    /// <returns><see langword="true"/> when a matching adapter is available; otherwise, <see langword="false"/>.</returns>
    bool TryGetBestAdapter(SpineVersionMatch versionMatch, out ISpineRuntimeAdapter? adapter);
}
