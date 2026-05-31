using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Creates durable session models from successful project loads.
/// </summary>
public interface ISpineSessionFactory
{
    /// <summary>
    /// Creates a session model from a successful load result.
    /// </summary>
    /// <param name="loadResult">The load result to convert into a session.</param>
    /// <returns>The durable application-facing session model.</returns>
    SpineProjectSession Create(LoadSpineProjectResult loadResult);
}
