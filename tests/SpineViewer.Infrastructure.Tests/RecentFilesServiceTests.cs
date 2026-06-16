using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Infrastructure.Persistence;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class RecentFilesServiceTests
{
    [Fact]
    public async Task AddAsync_PromotesProjectAndRespectsConfiguredLimitAsync()
    {
        string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string recentFilesPath = Path.Combine(tempDirectoryPath, "recent-files.json");

        try
        {
            Directory.CreateDirectory(tempDirectoryPath);
            RecentFilesService service = new(
                recentFilesPath,
                new FixedSettingsRepository(
                    new ViewerSettings(
                        ViewerTheme.FollowSystem,
                        true,
                        true,
                        false,
                        false,
                        true,
                        1.0,
                        2,
                        true,
                        null)));

            SpineProjectReference heroProject = new("Hero", "hero.json", "hero.atlas");
            SpineProjectReference mageProject = new("Mage", "mage.json", "mage.atlas");
            SpineProjectReference rogueProject = new("Rogue", "rogue.json", "rogue.atlas");

            await service.AddAsync(heroProject, CancellationToken.None);
            await service.AddAsync(mageProject, CancellationToken.None);
            await service.AddAsync(heroProject, CancellationToken.None);
            await service.AddAsync(rogueProject, CancellationToken.None);

            IReadOnlyList<SpineProjectReference> recentFiles = await service.GetRecentFilesAsync(CancellationToken.None);

            Assert.Equal(["Rogue", "Hero"], recentFiles.Select(static projectReference => projectReference.DisplayName));
        }
        finally
        {
            if (Directory.Exists(tempDirectoryPath))
            {
                Directory.Delete(tempDirectoryPath, true);
            }
        }
    }

    [Fact]
    public async Task RemoveMissingEntriesAsync_RemovesProjectsWhoseFilesNoLongerExistAsync()
    {
        string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string recentFilesPath = Path.Combine(tempDirectoryPath, "recent-files.json");

        try
        {
            Directory.CreateDirectory(tempDirectoryPath);
            string existingSkeletonPath = Path.Combine(tempDirectoryPath, "hero.json");
            string existingAtlasPath = Path.Combine(tempDirectoryPath, "hero.atlas");
            await File.WriteAllTextAsync(existingSkeletonPath, "{}", CancellationToken.None);
            await File.WriteAllTextAsync(existingAtlasPath, "atlas", CancellationToken.None);

            RecentFilesService service = new(recentFilesPath, new FixedSettingsRepository(new ViewerSettings()));

            await service.AddAsync(
                new SpineProjectReference("Hero", existingSkeletonPath, existingAtlasPath),
                CancellationToken.None);
            await service.AddAsync(
                new SpineProjectReference(
                    "Missing",
                    Path.Combine(tempDirectoryPath, "missing.json"),
                    Path.Combine(tempDirectoryPath, "missing.atlas")),
                CancellationToken.None);

            await service.RemoveMissingEntriesAsync(CancellationToken.None);

            IReadOnlyList<SpineProjectReference> recentFiles = await service.GetRecentFilesAsync(CancellationToken.None);
            Assert.Equal("Hero", Assert.Single(recentFiles).DisplayName);
        }
        finally
        {
            if (Directory.Exists(tempDirectoryPath))
            {
                Directory.Delete(tempDirectoryPath, true);
            }
        }
    }

    private sealed class FixedSettingsRepository : ISettingsRepository
    {
        public FixedSettingsRepository(ViewerSettings settings)
        {
            _settings = settings;
        }

        private readonly ViewerSettings _settings;

        public Task<ViewerSettings> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_settings);
        }

        public Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
