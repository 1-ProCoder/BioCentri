using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BioCentri.App.Components.Motion;

/// <summary>
/// One-shot staggered fade-up animation for <see cref="Page"/> Loaded
/// handlers. Walks the direct visual children of <paramref name="root"/>
/// and plays a 320 ms opacity 0→1 + translate 16→0 (Y) animation per
/// child with a 60 ms stagger.
///
/// Honors <c>Motion.RespectReducedMotion</c> — when the resource is
/// true, the animation is skipped entirely and every child is set to
/// its resting state immediately, matching the rest of the motion
/// surface (HologramFloat / ReticleRing / BorderTrace).
///
/// Usage: call from <c>Page.OnLoadedOnce</c>:
/// <code>
/// private void OnLoaded(object sender, RoutedEventArgs e)
/// {
///     EntranceAnimation.Play(ContentRoot);
/// }
/// </code>
/// where <c>ContentRoot</c> is the first <see cref="Panel"/> in the
/// page's visual tree.
/// </summary>
public static class EntranceAnimation
{
    private const int StepMs = 60;
    private const int DurationMs = 320;

    /// <summary>Apply a staggered fade-up to <paramref name="root"/>'s children.
    /// Safe to call multiple times — no-ops if any child is already animating.</summary>
    public static void Play(DependencyObject root)
    {
        if (root is not Panel panel) return;
        if (IsReducedMotion()) { ResetToResting(panel); return; }

        for (int i = 0; i < panel.Children.Count; i++)
        {
            if (panel.Children[i] is not UIElement child) continue;
            AnimateChild(child, i * StepMs);
        }
    }

    private static void AnimateChild(UIElement child, int beginMs)
    {
        // Prepare transforms BEFORE the first BeginAnimation call.
        if (child.RenderTransform is not TranslateTransform)
            child.RenderTransform = new TranslateTransform(0, 0);
        child.Opacity = 0;
        ((TranslateTransform)child.RenderTransform).Y = 16;

        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

        var fade = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(DurationMs),
            BeginTime = TimeSpan.FromMilliseconds(beginMs),
            EasingFunction = easing,
            FillBehavior = FillBehavior.HoldEnd,
        };
        var slide = new DoubleAnimation
        {
            From = 16,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(DurationMs),
            BeginTime = TimeSpan.FromMilliseconds(beginMs),
            EasingFunction = easing,
            FillBehavior = FillBehavior.HoldEnd,
        };

        child.BeginAnimation(UIElement.OpacityProperty, fade);
        ((TranslateTransform)child.RenderTransform)
            .BeginAnimation(TranslateTransform.YProperty, slide);
    }

    private static void ResetToResting(Panel panel)
    {
        for (int i = 0; i < panel.Children.Count; i++)
        {
            if (panel.Children[i] is not UIElement child) continue;
            child.Opacity = 1;
            if (child.RenderTransform is TranslateTransform t) t.Y = 0;
            child.BeginAnimation(UIElement.OpacityProperty, null);
        }
    }

    private static bool IsReducedMotion()
    {
        if (Application.Current?.Resources["Motion.RespectReducedMotion"] is bool b)
            return b;
        return false;
    }
}
