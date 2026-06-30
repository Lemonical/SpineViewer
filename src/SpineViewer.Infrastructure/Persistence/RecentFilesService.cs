using System.Text.Json;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Infrastructure.Serialization;

namespace SpineViewer.Infrastructure.Persistence;

/// <summary>
/// Stores the recent-file list in one JSON file and keeps it aligned with the configured settings limit.
/// </summary>
public sealed class RecentFilesService : IRecentFilesService
{
    private readonly string _recentFilesPath;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ISettingsRepository _settingsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecentFilesService"/> class.
    /// </summary>
    /// <param name="recentFilesPath">The absolute path to the persisted recent-files file.</param>
    /// <param name="settingsRepository">The settings repository that provides the recent-files limit.</param>
    public RecentFilesService(string recentFilesPath, ISettingsRepository settingsRepository)
    {
        _recentFilesPath = string.IsNullOrWhiteSpace(recentFilesPath)
            ? throw new ArgumentException("The recent-files path must not be null or whitespace.", nameof(recentFilesPath))
            : recentFilesPath;
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _serializerOptions = JsonSerializerOptionsFactory.CreateDefault();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SpineProjectReference>> GetRecentFilesAsync(CancellationToken cancellationToken)
    {
        return await LoadEntriesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task AddAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectReference);
        cancellationToken.ThrowIfCancellationRequested();

        ViewerSettings settings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
        List<SpineProjectReference> entries = [projectReference];
        entries.AddRange(
            (await LoadEntriesAsync(cancellationToken).ConfigureAwait(false))
            .Where(existingReference => !AreSameProject(existingReference, projectReference)));

        IReadOnlyList<SpineProjectReference> trimmedEntries = entries
            .Take(settings.RecentFilesLimit)
            .ToArray();

        await SaveEntriesAsync(trimmedEntries, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectReference);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<SpineProjectReference> filteredEntries = (await LoadEntriesAsync(cancellationToken).ConfigureAwait(false))
            .Where(existingReference => !AreSameProject(existingReference, projectReference))
            .ToArray();

        await SaveEntriesAsync(filteredEntries, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return SaveEntriesAsync(Array.Empty<SpineProjectReference>(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveMissingEntriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<SpineProjectReference> entries = await LoadEntriesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<SpineProjectReference> filteredEntries = entries
            .Where(
                static projectReference =>
                    File.Exists(projectReference.SkeletonPath) &&
                    File.Exists(projectReference.AtlasPath))
            .ToArray();

        if (filteredEntries.Count != entries.Count)
        {
            await SaveEntriesAsync(filteredEntries, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool AreSameProject(
        SpineProjectReference left,
        SpineProjectReference right)
    {
        return string.Equals(left.SkeletonPath, right.SkeletonPath, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(left.AtlasPath, right.AtlasPath, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyList<SpineProjectReference>> LoadEntriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_recentFilesPath))
        {
            return Array.Empty<SpineProjectReference>();
        }

        try
        {
            await using FileStream stream = File.OpenRead(_recentFilesPath);
            IReadOnlyList<SpineProjectReference>? entries = await JsonSerializer
                .DeserializeAsync<IReadOnlyList<SpineProjectReference>>(
                    stream,
                    _serializerOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            return entries?
                .Distinct()
                .ToArray() ?? Array.Empty<SpineProjectReference>();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return Array.Empty<SpineProjectReference>();
        }
    }

    private async Task SaveEntriesAsync(
        IReadOnlyList<SpineProjectReference> entries,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? directoryPath = Path.GetDirectoryName(_recentFilesPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using FileStream stream = File.Create(_recentFilesPath);
        await JsonSerializer
            .SerializeAsync(stream, entries, _serializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
