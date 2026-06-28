using Avalonia.Markup.Xaml;

namespace SpineViewer.Features.Settings.Views;

/// <summary>
/// Hosts the viewer settings surface.
/// </summary>
public partial class ViewerSettingsView : Avalonia.Controls.UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettingsView"/> class.
    /// </summary>
    public ViewerSettingsView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
