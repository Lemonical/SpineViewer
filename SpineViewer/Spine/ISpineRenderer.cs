using SkiaSharp;
using System.Collections.Generic;

namespace SpineViewer.Spine;

public interface ISpineRenderer
{
    void Initialize(params string[] path);
    void NextFrame(float delta);
    void Draw(SKCanvas canvas);
    void SetAnimation(SpineAnimation animation);
    void SetAnimations(params SpineAnimation[] animations);
    SpineAnimation[] GetCurrentAnimations();
    List<string> GetAnimations();
}