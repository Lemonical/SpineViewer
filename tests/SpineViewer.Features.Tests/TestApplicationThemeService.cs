using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Tests;

internal sealed class TestApplicationThemeService : IApplicationThemeService
{
    internal IReadOnlyList<ViewerTheme> AppliedThemes => _appliedThemes;

    private readonly List<ViewerTheme> _appliedThemes = [];

    public void ApplyTheme(ViewerTheme theme)
    {
        _appliedThemes.Add(theme);
    }
}
