using SpineViewer.Core.Models;
using SpineViewer.Features.Inspector.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class AssetInspectorViewModelTests
{
    [Fact]
    public void Constructor_UsesInspectionStateAndSelectedSkin()
    {
        SpineProjectInspection inspection = FeatureTestFactory.CreateInspection(
            animations: [new SpineAnimationInfo("idle", TimeSpan.FromSeconds(1), 2, 4)],
            skins:
            [
                new SpineSkinInfo("default", 1, 1, true),
                new SpineSkinInfo("winter", 1, 1, false),
            ],
            bones:
            [
                new SpineBoneInfo("root", null, 0, 0, 0, 0, 1, 1, 0),
                new SpineBoneInfo("hip", "root", 12, 1, 2, 3, 1, 1, 1),
            ],
            slots: [new SpineSlotInfo("body", "hip", "body-default", "normal")],
            attachments:
            [
                new SpineAttachmentInfo("default", "body", "body-default", "region", "body-default", 0, 0, 0, 1, 1, 64, 32, 0, 0),
                new SpineAttachmentInfo("winter", "body", "body-winter", "region", "body-winter", 0, 0, 0, 1, 1, 64, 32, 0, 0),
            ],
            atlasPages: [new SpineAtlasPageInfo("hero.png", 512, 256, "RGBA8888", "Linear,Linear", "none", 1)],
            atlasRegions: [new SpineAtlasRegionInfo("hero.png", "body-default", false, 64, 32, 64, 32)]);
        TestWorkspaceSessionService workspaceSessionService = new(
            FeatureTestFactory.CreateWorkspaceState(
                FeatureTestFactory.CreateSession(
                    inspection: inspection,
                    selectedSkinName: "winter")));

        AssetInspectorViewModel viewModel = new(workspaceSessionService);

        Assert.Equal("winter", viewModel.SelectedSkin?.Name);
        Assert.Equal("body-winter", Assert.IsType<SpineAttachmentInfo>(viewModel.SelectedAttachment).Name);
        Assert.Equal("idle", Assert.IsType<SpineAnimationInfo>(viewModel.SelectedAnimation).Name);
        BoneTreeNodeViewModel rootBone = Assert.Single(viewModel.BoneTree);
        Assert.Equal("root", rootBone.Name);
        Assert.Single(rootBone.Children);
        Assert.Contains("1 animation(s)", viewModel.ExportSummaryText);
    }

    [Fact]
    public void SelectingSkin_UpdatesAttachmentSelectionAndWorkspaceSession()
    {
        SpineProjectInspection inspection = FeatureTestFactory.CreateInspection(
            skins:
            [
                new SpineSkinInfo("default", 1, 1, true),
                new SpineSkinInfo("winter", 1, 1, false),
            ],
            attachments:
            [
                new SpineAttachmentInfo("default", "body", "body-default", "region", "body-default", 0, 0, 0, 1, 1, 64, 32, 0, 0),
                new SpineAttachmentInfo("winter", "body", "body-winter", "region", "body-winter", 0, 0, 0, 1, 1, 64, 32, 0, 0),
            ]);
        TestWorkspaceSessionService workspaceSessionService = new(
            FeatureTestFactory.CreateWorkspaceState(
                FeatureTestFactory.CreateSession(
                    inspection: inspection,
                    selectedSkinName: "default")));
        AssetInspectorViewModel viewModel = new(workspaceSessionService);

        viewModel.SelectedSkin = viewModel.Skins[1];

        Assert.Equal("winter", Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).SelectedSkinName);
        Assert.Equal("body-winter", Assert.IsType<SpineAttachmentInfo>(viewModel.SelectedAttachment).Name);
        Assert.All(viewModel.SkinAttachments, static attachment => Assert.Equal("winter", attachment.SkinName));
    }
}
