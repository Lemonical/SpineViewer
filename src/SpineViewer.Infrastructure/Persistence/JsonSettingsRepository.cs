using System.Text.Json;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Infrastructure.Serialization;

namespace SpineViewer.Infrastructure.Persistence;

/// <summary>
/// Stores viewer settings in one JSON file under the local application data directory.
/// </summary>
public sealed class JsonSettingsRepository : ISettingsRepository
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly string _settingsPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSettingsRepository"/> class.
    /// </summary>
    /// <param name="settingsPath">The absolute path to the persisted settings file.</param>
    public JsonSettingsRepository(string settingsPath)
    {
        _settingsPath = string.IsNullOrWhiteSpace(settingsPath)
            ? throw new ArgumentException("The settings path must not be null or whitespace.", nameof(settingsPath))
            : settingsPath;
        _serializerOptions = JsonSerializerOptionsFactory.CreateDefault();
    }

    /// <inheritdoc />
    public async Task<ViewerSettings> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(_settingsPath))
        {
            return new ViewerSettings();
        }

        try
        {
            await using FileStream stream = File.OpenRead(_settingsPath);
            ViewerSettings? settings = await JsonSerializer
                .DeserializeAsync<ViewerSettings>(stream, _serializerOptions, cancellationToken)
                .ConfigureAwait(false);

            return settings ?? new ViewerSettings();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return new ViewerSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        string? directoryPath = Path.GetDirectoryName(_settingsPath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using FileStream stream = File.Create(_settingsPath);
        await JsonSerializer
            .SerializeAsync(stream, settings, _serializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
