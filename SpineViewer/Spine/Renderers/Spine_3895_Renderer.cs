using SkiaSharp;
using Spine_3895;
using Spine_3895.Skia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpineViewer.Spine.Renderers;

public class Spine_3895_Renderer : ISpineRenderer
{
    private AnimationState _animationState;
    private Skeleton _skeleton;
    private SkeletonData _skeletonData;
    private SkeletonRenderer _skeletonRenderer;

    public void Initialize(params string[] path)
    {
        if (path.Length != 2 || path.ElementAtOrDefault(0) is not string atlasFile ||
            path.ElementAtOrDefault(1) is not string skeletonFile ||
            !File.Exists(atlasFile) || !File.Exists(skeletonFile))
            throw new InvalidOperationException();

        Bone.yDown = true;

        var textureLoader = new SpineTextureLoader();
        var atlas = new Atlas(atlasFile, textureLoader);
        var attachmentLoader = new AtlasAttachmentLoader(atlas);

        if (Path.GetExtension(skeletonFile) == ".json")
        {
            var skeletonJson = new SkeletonJson(attachmentLoader)
            {
                Scale = 1
            };
            _skeletonData = skeletonJson.ReadSkeletonData(skeletonFile);
        }
        else
        {
            var skeletonBinary = new SkeletonBinary(attachmentLoader)
            {
                Scale = 1
            };
            _skeletonData = skeletonBinary.ReadSkeletonData(skeletonFile);
        }

        var animationStateData = new AnimationStateData(_skeletonData)
        {
            DefaultMix = .3f
        };

        _animationState = new AnimationState(animationStateData);
        _skeleton = new Skeleton(_skeletonData);
        _skeleton.UpdateWorldTransform();

        _skeletonRenderer = new();
    }

    public void SetAnimation(SpineAnimation animation)
    {
        _animationState.ClearTracks();
        SetAnimationInternal(animation);
    }

    public void SetAnimations(params SpineAnimation[] animations)
    {
        if (animations == null || animations.Length == 0)
            throw new ArgumentNullException();

        _animationState.ClearTracks();
        foreach (var animation in animations)
            SetAnimationInternal(animation);
    }

    private void SetAnimationInternal(SpineAnimation animation)
    {
        var state = _animationState.SetAnimation(animation.TrackIndex, animation.Name, animation.Loop);
        state.TimeScale = animation.Timescale;
    }

    public SpineAnimation[] GetCurrentAnimations()
    {
        return [.. _animationState.Tracks
            .Select(x => new SpineAnimation(x.TrackIndex, x.Animation.Name, x.Loop, x.TimeScale))];
    }

    public List<string> GetAnimations()
        => _skeletonData.Animations.Select(x => x.Name).ToList();

    public void Draw(SKCanvas canvas)
        => _skeletonRenderer?.Draw(_skeleton, canvas);

    public void NextFrame(float delta)
    {
        _animationState.Update(delta);
        _animationState.Apply(_skeleton);
        _skeleton.UpdateWorldTransform();
    }
}
