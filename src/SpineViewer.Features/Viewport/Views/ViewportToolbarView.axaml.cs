using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpineViewer.Features.Viewport.Views;

/// <summary>
/// Hosts the viewport interaction toolbar.
/// </summary>
public partial class ViewportToolbarView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportToolbarView"/> class.
    /// </summary>
    public ViewportToolbarView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
