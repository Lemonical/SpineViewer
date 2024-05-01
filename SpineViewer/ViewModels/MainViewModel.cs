using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpineViewer.Spine;
using SpineViewer.Spine.Renderers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadSpineModelCommand))]
    private string _atlasFile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadSpineModelCommand))]
    private string _skeletonFile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAnimationTrackCommand))]
    private ISpineRenderer _spineRenderer;

    [ObservableProperty]
    private float _scale = 1f;

    [ObservableProperty]
    private List<string> _animations;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteAnimationTrackCommand))]
    [NotifyPropertyChangedFor(nameof(HasCurrentTrackSelected))]
    private SpineAnimation? _currentTrack;

    public bool HasCurrentTrackSelected => CurrentTrack != null;

    [ObservableProperty]
    private SpineAnimation[] _tracks = [];

    public List<string> SpineVersions { get; }

    [ObservableProperty]
    private string _selectedSpineVersion;

    public MainViewModel()
    {
        if (Design.IsDesignMode)
        {
            Tracks = [new(0, "Idle", false, 1f), new(1, "Blink", false, 1f)];
            Animations = ["Idle", "Blink"];
        }
        SpineVersions = typeof(SpineVersion)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(x => x.GetCustomAttribute<EnumMemberAttribute>()?.Value)
            .ToList()!;

        SelectedSpineVersion = SpineVersions[0];
    }

    [RelayCommand]
    public void TogglePanel()
        => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    public async Task OpenAtlasFileDialogAsync()
        => AtlasFile = await OpenFileDialogAsync("Select Atlas file", "Atlas", "*.atlas");

    [RelayCommand]
    public async Task OpenSkelFileDialogAsync()
        => SkeletonFile = await OpenFileDialogAsync("Select Skeleton file", "Skeleton Data", "*.skel|*.json");

    [RelayCommand(CanExecute = nameof(CanAddAnimationTrack))]
    public void AddAnimationTrack()
    {
        var maxIndex = Tracks.Length < 1 ? 0 : Tracks.MaxBy(x => x.TrackIndex)!.TrackIndex + 1;
        var track = new SpineAnimation(maxIndex, Animations[0], true, 1f);

        Tracks = [.. Tracks, track];
        CurrentTrack = track;

    }
    private bool CanAddAnimationTrack()
        => SpineRenderer != null;

    [RelayCommand(CanExecute = nameof(CanDeleteAnimationTrack))]
    public void DeleteAnimationTrack()
    {
        var tracks = Tracks.ToList();
        tracks.Remove(CurrentTrack!);
        Tracks = [.. tracks];
        if (Tracks.Length > 0)
            CurrentTrack = Tracks[0];
        else
            CurrentTrack = null;
    }
    private bool CanDeleteAnimationTrack()
        => CurrentTrack != null;

    [RelayCommand]
    public void ApplyAnimations()
    {
        Tracks = [.. Tracks];
    }

    [RelayCommand(CanExecute = nameof(CanLoadSpineModel))]
    public void LoadSpineModel()
    {
        Tracks = [];
        var version = GetSpineVersion(SelectedSpineVersion);

        ISpineRenderer renderer = version switch
        {
            SpineVersion.Spine41 => new Spine_4100_Renderer(),
            _ => new Spine_3895_Renderer(),
        };

        renderer.Initialize(AtlasFile, SkeletonFile);
        Animations = renderer.GetAnimations();
        SpineRenderer = renderer;
        AddAnimationTrackCommand.Execute(this);
    }
    private bool CanLoadSpineModel()
        => !string.IsNullOrWhiteSpace(AtlasFile) && !string.IsNullOrWhiteSpace(SkeletonFile);

    private async Task<string> OpenFileDialogAsync(string title, string name, string extensions)
    {
        var topLevel = App.GetTopLevel()!;
        List<string> extensionList = extensions.Contains('|')
            ? new(extensions.Split('|'))
            : [extensions];

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType(name) { Patterns = extensionList } ],
        });

        if (files.Count == 1)
            return files[0].Path.LocalPath;
        else return string.Empty;
    }

    private SpineVersion GetSpineVersion(string version)
    {
        return (SpineVersion)typeof(SpineVersion)
            .GetFields()
            .FirstOrDefault(x => x.GetCustomAttributes<EnumMemberAttribute>(false)
            .Cast<EnumMemberAttribute>()
            .Any(a => a.Value == version))!
            .GetValue(null)!;
    }
}
