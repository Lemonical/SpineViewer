using Avalonia.Markup.Xaml;

namespace SpineViewer.Features.Inspector.Views;

/// <summary>
/// Hosts the structured asset inspector surface.
/// </summary>
public partial class AssetInspectorView : Avalonia.Controls.UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetInspectorView"/> class.
    /// </summary>
    public AssetInspectorView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
