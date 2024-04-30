using CommunityToolkit.Mvvm.ComponentModel;

namespace SpineViewer.Spine;

public partial class SpineAnimation(int trackIndex, string name, bool loop, float timescale) : ObservableObject
{
    [ObservableProperty]
    private int _trackIndex = trackIndex;

    [ObservableProperty]
    private string _name = name;

    [ObservableProperty]
    private bool _loop = loop;

    [ObservableProperty]
    private float _timescale = timescale;
}