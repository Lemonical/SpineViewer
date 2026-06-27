using Avalonia.Markup.Xaml;

namespace SpineViewer.Features.Diagnostics.Views;

/// <summary>
/// Hosts the diagnostics and warning panel.
/// </summary>
public partial class DiagnosticsPanelView : Avalonia.Controls.UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsPanelView"/> class.
    /// </summary>
    public DiagnosticsPanelView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
