using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Resolves selected Spine files or stored project references into a concrete asset file set.
/// </summary>
public sealed class SpineProjectReferenceResolver : ISpineProjectReferenceResolver
{
    private static readonly string[] SkeletonExtensions = [".json", ".skel", ".bytes"];

    /// <inheritdoc />
    public Task<ResolveSpineProjectResult> ResolveFromSelectionAsync(
        string selectedPath,
        string? companionPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedSelectedPath = Path.GetFullPath(selectedPath);
        string? normalizedCompanionPath = string.IsNullOrWhiteSpace(companionPath)
            ? null
            : Path.GetFullPath(companionPath);

        List<ViewerDiagnostic> diagnostics = [];

        FileRole selectedRole = GetFileRole(normalizedSelectedPath);
        if (selectedRole == FileRole.Unsupported)
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "project-resolution-unsupported-selection",
                    ViewerDiagnosticSeverity.Error,
                    "The selected file is not a supported Spine atlas or skeleton file.",
                    nameof(SpineProjectReferenceResolver),
                    $"Selected path: '{normalizedSelectedPath}'.",
                    "Select a .atlas, .json, .skel, or .bytes file."));

            return Task.FromResult(new ResolveSpineProjectResult(false, null, null, diagnostics));
        }

        if (normalizedCompanionPath is not null)
        {
            FileRole companionRole = GetFileRole(normalizedCompanionPath);
            if (companionRole == FileRole.Unsupported || companionRole == selectedRole)
            {
                diagnostics.Add(
                    new ViewerDiagnostic(
                        "project-resolution-invalid-companion-selection",
                        ViewerDiagnosticSeverity.Error,
                        "The companion file selection is not a valid atlas and skeleton pair.",
                        nameof(SpineProjectReferenceResolver),
                        $"Selected path: '{normalizedSelectedPath}'. Companion path: '{normalizedCompanionPath}'.",
                        "Select exactly one atlas file and one supported skeleton file."));

                return Task.FromResult(new ResolveSpineProjectResult(false, null, null, diagnostics));
            }
        }

        string skeletonPath;
        string atlasPath;

        if (selectedRole == FileRole.Skeleton)
        {
            skeletonPath = normalizedSelectedPath;
            atlasPath = normalizedCompanionPath ?? ResolveAtlasPath(skeletonPath, diagnostics);
        }
        else
        {
            atlasPath = normalizedSelectedPath;
            skeletonPath = normalizedCompanionPath ?? ResolveSkeletonPath(atlasPath, diagnostics);
        }

        SpineProjectReference projectReference = CreateProjectReference(skeletonPath, atlasPath);
        ResolveSpineProjectResult result = ResolveProject(projectReference, diagnostics, cancellationToken);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<ResolveSpineProjectResult> ResolveForReopenAsync(
        SpineProjectReference projectReference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectReference);
        cancellationToken.ThrowIfCancellationRequested();

        SpineProjectReference normalizedReference = new(
            projectReference.DisplayName,
            Path.GetFullPath(projectReference.SkeletonPath),
            Path.GetFullPath(projectReference.AtlasPath));

        ResolveSpineProjectResult result = ResolveProject(normalizedReference, [], cancellationToken);
        return Task.FromResult(result);
    }

    private static ResolveSpineProjectResult ResolveProject(
        SpineProjectReference projectReference,
        List<ViewerDiagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(projectReference.SkeletonPath))
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "project-resolution-skeleton-missing",
                    ViewerDiagnosticSeverity.Error,
                    "The skeleton file could not be found.",
                    nameof(SpineProjectReferenceResolver),
                    $"Missing skeleton path: '{projectReference.SkeletonPath}'.",
                    "Restore the skeleton file or choose a different project."));
        }

        if (!File.Exists(projectReference.AtlasPath))
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "project-resolution-atlas-missing",
                    ViewerDiagnosticSeverity.Error,
                    "The atlas file could not be found.",
                    nameof(SpineProjectReferenceResolver),
                    $"Missing atlas path: '{projectReference.AtlasPath}'.",
                    "Restore the atlas file or choose a different project."));
        }

        if (HasBlockingDiagnostics(diagnostics))
        {
            return new ResolveSpineProjectResult(false, projectReference, null, diagnostics);
        }

        IReadOnlyList<string> texturePaths = ResolveTextureDependencies(projectReference.AtlasPath, diagnostics);
        SpineAssetFileSet? assetFileSet = texturePaths.Count == 0
            ? null
            : new SpineAssetFileSet(projectReference.SkeletonPath, projectReference.AtlasPath, texturePaths);

        bool isSuccessful = assetFileSet is not null && !HasBlockingDiagnostics(diagnostics);
        return new ResolveSpineProjectResult(isSuccessful, projectReference, assetFileSet, diagnostics);
    }

    private static IReadOnlyList<string> ResolveTextureDependencies(
        string atlasPath,
        List<ViewerDiagnostic> diagnostics)
    {
        List<string> texturePaths = [];
        string atlasDirectory = Path.GetDirectoryName(atlasPath)
            ?? throw new InvalidOperationException("The atlas directory could not be determined.");

        string[] atlasLines;

        try
        {
            atlasLines = File.ReadAllLines(atlasPath);
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "atlas-read-failed",
                    ViewerDiagnosticSeverity.Error,
                    "The atlas file could not be read.",
                    nameof(SpineProjectReferenceResolver),
                    exception.Message,
                    "Verify that the atlas file is accessible and not locked by another process."));

            return texturePaths;
        }

        bool expectingPageName = true;

        foreach ((string rawLine, int lineNumber) in atlasLines.Select(static (line, index) => (line, index + 1)))
        {
            string line = rawLine.Trim();

            if (line.Length == 0)
            {
                expectingPageName = true;
                continue;
            }

            if (expectingPageName)
            {
                if (line.Contains(':'))
                {
                    diagnostics.Add(
                        new ViewerDiagnostic(
                            "atlas-page-missing-before-entry",
                            ViewerDiagnosticSeverity.Error,
                            "The atlas file contains an entry before any atlas page name.",
                            nameof(SpineProjectReferenceResolver),
                            $"Atlas '{atlasPath}' has an invalid entry on line {lineNumber}: '{rawLine}'.",
                            "Re-export the atlas or repair the atlas page declaration."));

                    return [];
                }

                string resolvedTexturePath = Path.GetFullPath(Path.Combine(atlasDirectory, line));
                texturePaths.Add(resolvedTexturePath);
                expectingPageName = false;
                continue;
            }

            if (!line.Contains(':'))
            {
                continue;
            }
        }

        if (texturePaths.Count == 0)
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "atlas-no-pages",
                    ViewerDiagnosticSeverity.Error,
                    "The atlas file does not declare any texture pages.",
                    nameof(SpineProjectReferenceResolver),
                    $"Atlas path: '{atlasPath}'.",
                    "Re-export the atlas and verify that it includes at least one page."));

            return [];
        }

        foreach (string texturePath in texturePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(texturePath))
            {
                diagnostics.Add(
                    new ViewerDiagnostic(
                        "atlas-texture-missing",
                        ViewerDiagnosticSeverity.Error,
                        "A texture referenced by the atlas could not be found.",
                        nameof(SpineProjectReferenceResolver),
                        $"Missing texture path: '{texturePath}'. Atlas path: '{atlasPath}'.",
                        "Restore the missing texture or re-export the atlas with valid page paths."));
            }
        }

        return texturePaths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string ResolveAtlasPath(
        string skeletonPath,
        List<ViewerDiagnostic> diagnostics)
    {
        string atlasPath = Path.ChangeExtension(skeletonPath, ".atlas");
        if (File.Exists(atlasPath))
        {
            return atlasPath;
        }

        diagnostics.Add(
            new ViewerDiagnostic(
                "project-resolution-atlas-auto-pair-missing",
                ViewerDiagnosticSeverity.Error,
                "The companion atlas file could not be located automatically.",
                nameof(SpineProjectReferenceResolver),
                $"Expected atlas path: '{atlasPath}'.",
                "Select the atlas file explicitly or place it next to the skeleton with the same base name."));

        return atlasPath;
    }

    private static string ResolveSkeletonPath(
        string atlasPath,
        List<ViewerDiagnostic> diagnostics)
    {
        string basePath = Path.Combine(
            Path.GetDirectoryName(atlasPath)
                ?? throw new InvalidOperationException("The atlas directory could not be determined."),
            Path.GetFileNameWithoutExtension(atlasPath));

        List<string> candidates = SkeletonExtensions
            .Select(extension => basePath + extension)
            .Where(File.Exists)
            .ToList();

        if (candidates.Count == 0)
        {
            string expectedPathList = string.Join(", ", SkeletonExtensions.Select(extension => basePath + extension));
            diagnostics.Add(
                new ViewerDiagnostic(
                    "project-resolution-skeleton-auto-pair-missing",
                    ViewerDiagnosticSeverity.Error,
                    "The companion skeleton file could not be located automatically.",
                    nameof(SpineProjectReferenceResolver),
                    $"Expected one of: {expectedPathList}.",
                    "Select the skeleton file explicitly or place it next to the atlas with the same base name."));

            return basePath + ".json";
        }

        if (candidates.Count > 1)
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "project-resolution-skeleton-auto-pair-ambiguous",
                    ViewerDiagnosticSeverity.Warning,
                    "Multiple companion skeleton files were found. The first supported candidate was selected automatically.",
                    nameof(SpineProjectReferenceResolver),
                    $"Selected skeleton path: '{candidates[0]}'. Other candidates: {string.Join(", ", candidates.Skip(1))}.",
                    "Select the skeleton file explicitly if the automatic choice is incorrect."));
        }

        return candidates[0];
    }

    private static SpineProjectReference CreateProjectReference(string skeletonPath, string atlasPath)
    {
        return new SpineProjectReference(
            Path.GetFileNameWithoutExtension(skeletonPath),
            skeletonPath,
            atlasPath);
    }

    private static bool HasBlockingDiagnostics(IEnumerable<ViewerDiagnostic> diagnostics)
    {
        return diagnostics.Any(static diagnostic => diagnostic.Severity == ViewerDiagnosticSeverity.Error);
    }

    private static FileRole GetFileRole(string path)
    {
        string extension = Path.GetExtension(path);

        if (string.Equals(extension, ".atlas", StringComparison.OrdinalIgnoreCase))
        {
            return FileRole.Atlas;
        }

        if (SkeletonExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return FileRole.Skeleton;
        }

        return FileRole.Unsupported;
    }

    private enum FileRole
    {
        Unsupported,
        Atlas,
        Skeleton,
    }
}
