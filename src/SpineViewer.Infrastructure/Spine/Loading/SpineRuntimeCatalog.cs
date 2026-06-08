using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Stores the runtime adapters available to the rewrite host.
/// </summary>
public sealed class SpineRuntimeCatalog : ISpineRuntimeCatalog
{
    private readonly IReadOnlyDictionary<string, ISpineRuntimeAdapter> _adaptersByRuntimeId;
    private readonly IReadOnlyList<SpineRuntimeDescriptor> _descriptors;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimeCatalog"/> class.
    /// </summary>
    /// <param name="adapters">The runtime adapters to register.</param>
    public SpineRuntimeCatalog(IEnumerable<ISpineRuntimeAdapter> adapters)
    {
        ArgumentNullException.ThrowIfNull(adapters);

        Dictionary<string, ISpineRuntimeAdapter> adaptersByRuntimeId = new(StringComparer.OrdinalIgnoreCase);
        List<SpineRuntimeDescriptor> descriptors = [];

        foreach (ISpineRuntimeAdapter adapter in adapters)
        {
            if (!adaptersByRuntimeId.TryAdd(adapter.Descriptor.RuntimeId, adapter))
            {
                throw new ArgumentException(
                    $"A runtime adapter with the id '{adapter.Descriptor.RuntimeId}' is already registered.",
                    nameof(adapters));
            }

            descriptors.Add(adapter.Descriptor);
        }

        _adaptersByRuntimeId = adaptersByRuntimeId;
        _descriptors = descriptors;
    }

    /// <inheritdoc />
    public IReadOnlyList<SpineRuntimeDescriptor> GetAvailableRuntimes()
    {
        return _descriptors;
    }

    /// <inheritdoc />
    public ISpineRuntimeAdapter GetRequiredAdapter(string runtimeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeId);

        if (_adaptersByRuntimeId.TryGetValue(runtimeId, out ISpineRuntimeAdapter? adapter))
        {
            return adapter;
        }

        throw new KeyNotFoundException($"No runtime adapter is registered for '{runtimeId}'.");
    }

    /// <inheritdoc />
    public bool TryGetBestAdapter(SpineVersionMatch versionMatch, out ISpineRuntimeAdapter? adapter)
    {
        ArgumentNullException.ThrowIfNull(versionMatch);

        IEnumerable<string> runtimeIds = versionMatch.SuggestedRuntimeId is null
            ? versionMatch.CompatibleRuntimeIds
            : new[] { versionMatch.SuggestedRuntimeId }.Concat(versionMatch.CompatibleRuntimeIds);

        foreach (string runtimeId in runtimeIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (_adaptersByRuntimeId.TryGetValue(runtimeId, out adapter))
            {
                return true;
            }
        }

        adapter = null;
        return false;
    }
}
