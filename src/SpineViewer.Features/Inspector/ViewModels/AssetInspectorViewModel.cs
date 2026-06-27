using CommunityToolkit.Mvvm.ComponentModel;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Inspector.ViewModels;

/// <summary>
/// Presents structured Spine asset inspection data for the current session.
/// </summary>
public sealed partial class AssetInspectorViewModel : ObservableObject, IDisposable
{
    private readonly IWorkspaceSessionService _workspaceSessionService;
    private SpineProjectSession? _currentSession;
    private bool _isSynchronizingSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetInspectorViewModel"/> class.
    /// </summary>
    public AssetInspectorViewModel(IWorkspaceSessionService workspaceSessionService)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));

        ApplyState(_workspaceSessionService.State);
        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Gets a value indicating whether a session is currently active.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    private bool hasActiveSession;

    /// <summary>
    /// Gets the currently available animations.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineAnimationInfo> animations = Array.Empty<SpineAnimationInfo>();

    /// <summary>
    /// Gets the currently available skins.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineSkinInfo> skins = Array.Empty<SpineSkinInfo>();

    /// <summary>
    /// Gets the currently available slots.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineSlotInfo> slots = Array.Empty<SpineSlotInfo>();

    /// <summary>
    /// Gets the currently available atlas pages.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineAtlasPageInfo> atlasPages = Array.Empty<SpineAtlasPageInfo>();

    /// <summary>
    /// Gets the currently available atlas regions.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineAtlasRegionInfo> atlasRegions = Array.Empty<SpineAtlasRegionInfo>();

    /// <summary>
    /// Gets the current runtime capability entries.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineRuntimeFeatureSupport> runtimeCapabilities = Array.Empty<SpineRuntimeFeatureSupport>();

    /// <summary>
    /// Gets the current unsupported features.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<UnsupportedSpineFeature> unsupportedFeatures = Array.Empty<UnsupportedSpineFeature>();

    /// <summary>
    /// Gets the current attachment list for the selected skin.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<SpineAttachmentInfo> skinAttachments = Array.Empty<SpineAttachmentInfo>();

    /// <summary>
    /// Gets the current bones tree.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<BoneTreeNodeViewModel> boneTree = Array.Empty<BoneTreeNodeViewModel>();

    /// <summary>
    /// Gets the current export metadata summary.
    /// </summary>
    [ObservableProperty]
    private SpineExportMetadata exportMetadata = new(null, null, null, null, null, null, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Gets the currently selected animation.
    /// </summary>
    [ObservableProperty]
    private SpineAnimationInfo? selectedAnimation;

    /// <summary>
    /// Gets the currently selected skin.
    /// </summary>
    [ObservableProperty]
    private SpineSkinInfo? selectedSkin;

    /// <summary>
    /// Gets the currently selected slot.
    /// </summary>
    [ObservableProperty]
    private SpineSlotInfo? selectedSlot;

    /// <summary>
    /// Gets the currently selected attachment.
    /// </summary>
    [ObservableProperty]
    private SpineAttachmentInfo? selectedAttachment;

    /// <summary>
    /// Gets the currently selected atlas page.
    /// </summary>
    [ObservableProperty]
    private SpineAtlasPageInfo? selectedAtlasPage;

    /// <summary>
    /// Releases the workspace-state subscription held by the inspector.
    /// </summary>
    public void Dispose()
    {
        _workspaceSessionService.StateChanged -= OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Gets the guidance text shown when no session is open.
    /// </summary>
    public string EmptyStateText => HasActiveSession
        ? "Choose an inspector tab to explore animations, skins, bones, slots, atlas data, and runtime support."
        : "Open a Spine project to inspect its exported structure.";

    /// <summary>
    /// Gets the export summary text.
    /// </summary>
    public string ExportSummaryText =>
        $"Spine {ExportMetadata.ExportVersion ?? "Unknown"} | " +
        $"{ExportMetadata.AnimationCount} animation(s) | " +
        $"{ExportMetadata.SkinCount} skin(s) | " +
        $"{ExportMetadata.BoneCount} bone(s)";

    partial void OnSelectedSkinChanged(SpineSkinInfo? value)
    {
        RefreshSkinAttachments(value);
        SelectedAttachment = ChooseSelectedAttachment();

        if (_isSynchronizingSelection || _currentSession is null)
        {
            return;
        }

        _workspaceSessionService.UpdateSelectedSkin(value?.Name);
    }

    private void ApplyState(WorkspaceState workspaceState)
    {
        _currentSession = workspaceState.CurrentSession;
        HasActiveSession = _currentSession is not null;

        SpineProjectInspection inspection = _currentSession?.Inspection ?? SpineProjectInspection.Empty;
        Animations = inspection.Animations;
        Skins = inspection.Skins;
        Slots = inspection.Slots;
        AtlasPages = inspection.AtlasPages;
        AtlasRegions = inspection.AtlasRegions;
        RuntimeCapabilities = _currentSession?.Runtime.Capabilities.FeatureSupport ?? Array.Empty<SpineRuntimeFeatureSupport>();
        UnsupportedFeatures = _currentSession?.UnsupportedFeatures ?? Array.Empty<UnsupportedSpineFeature>();
        ExportMetadata = inspection.ExportMetadata;
        BoneTree = BuildBoneTree(inspection.Bones);

        _isSynchronizingSelection = true;

        try
        {
            SelectedAnimation = ChooseSelectedAnimation();
            SelectedSkin = ChooseSelectedSkin();
            SelectedSlot = ChooseSelectedSlot();
            SelectedAtlasPage = ChooseSelectedAtlasPage();
        }
        finally
        {
            _isSynchronizingSelection = false;
        }

        RefreshSkinAttachments(SelectedSkin);
        SelectedAttachment = ChooseSelectedAttachment();
        OnPropertyChanged(nameof(ExportSummaryText));
    }

    private static IReadOnlyList<BoneTreeNodeViewModel> BuildBoneTree(IReadOnlyList<SpineBoneInfo> bones)
    {
        if (bones.Count == 0)
        {
            return Array.Empty<BoneTreeNodeViewModel>();
        }

        Dictionary<string, IReadOnlyList<SpineBoneInfo>> childrenByParent = bones
            .Where(static bone => !string.IsNullOrWhiteSpace(bone.ParentName))
            .GroupBy(static bone => bone.ParentName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => (IReadOnlyList<SpineBoneInfo>)group.OrderBy(static bone => bone.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return bones
            .Where(static bone => string.IsNullOrWhiteSpace(bone.ParentName))
            .OrderBy(static bone => bone.Name, StringComparer.OrdinalIgnoreCase)
            .Select(BuildNode)
            .ToArray();

        BoneTreeNodeViewModel BuildNode(SpineBoneInfo bone)
        {
            childrenByParent.TryGetValue(bone.Name, out IReadOnlyList<SpineBoneInfo>? children);

            return new BoneTreeNodeViewModel(
                bone.Name,
                bone.Length,
                children?.Select(BuildNode) ?? Array.Empty<BoneTreeNodeViewModel>());
        }
    }

    private SpineAnimationInfo? ChooseSelectedAnimation()
    {
        if (Animations.Count == 0)
        {
            return null;
        }

        if (SelectedAnimation is null)
        {
            return Animations[0];
        }

        return Animations.FirstOrDefault(animation => animation.Name == SelectedAnimation.Name) ?? Animations[0];
    }

    private SpineAtlasPageInfo? ChooseSelectedAtlasPage()
    {
        if (AtlasPages.Count == 0)
        {
            return null;
        }

        if (SelectedAtlasPage is null)
        {
            return AtlasPages[0];
        }

        return AtlasPages.FirstOrDefault(page => page.Name == SelectedAtlasPage.Name) ?? AtlasPages[0];
    }

    private SpineAttachmentInfo? ChooseSelectedAttachment()
    {
        if (SkinAttachments.Count == 0)
        {
            return null;
        }

        if (SelectedAttachment is null)
        {
            return SkinAttachments[0];
        }

        return SkinAttachments.FirstOrDefault(
                   attachment =>
                       attachment.Name == SelectedAttachment.Name &&
                       attachment.SlotName == SelectedAttachment.SlotName &&
                       attachment.SkinName == SelectedAttachment.SkinName) ??
               SkinAttachments[0];
    }

    private SpineSkinInfo? ChooseSelectedSkin()
    {
        if (Skins.Count == 0)
        {
            return null;
        }

        string? selectedSkinName = _currentSession?.SelectedSkinName;
        if (!string.IsNullOrWhiteSpace(selectedSkinName))
        {
            return Skins.FirstOrDefault(skin => string.Equals(skin.Name, selectedSkinName, StringComparison.OrdinalIgnoreCase)) ?? Skins[0];
        }

        if (SelectedSkin is null)
        {
            return Skins.FirstOrDefault(static skin => skin.IsDefault) ?? Skins[0];
        }

        return Skins.FirstOrDefault(skin => skin.Name == SelectedSkin.Name) ?? Skins[0];
    }

    private SpineSlotInfo? ChooseSelectedSlot()
    {
        if (Slots.Count == 0)
        {
            return null;
        }

        if (SelectedSlot is null)
        {
            return Slots[0];
        }

        return Slots.FirstOrDefault(slot => slot.Name == SelectedSlot.Name) ?? Slots[0];
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        ApplyState(_workspaceSessionService.State);
    }

    private void RefreshSkinAttachments(SpineSkinInfo? skin)
    {
        SkinAttachments = _currentSession?.Inspection.Attachments
            .Where(
                attachment =>
                    skin is not null &&
                    string.Equals(attachment.SkinName, skin.Name, StringComparison.OrdinalIgnoreCase))
            .ToArray() ?? Array.Empty<SpineAttachmentInfo>();
    }
}
