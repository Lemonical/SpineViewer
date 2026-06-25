using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpineViewer.Features.Playback.Views;

/// <summary>
/// Hosts the playback transport controls.
/// </summary>
public partial class PlaybackTransportView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackTransportView"/> class.
    /// </summary>
    public PlaybackTransportView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
