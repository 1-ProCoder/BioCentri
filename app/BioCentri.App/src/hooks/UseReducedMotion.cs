namespace BioCentri.App.Hooks;

/// <summary>
/// Reduced-motion toggle. Mirrors the website's
/// <c>@media (prefers-reduced-motion: reduce)</c> rule. Windows has no
/// first-class equivalent; for Milestone 1 this is a static toggle the
/// app can flip programmatically. Real settings wiring lands in M5.
/// </summary>
public static class UseReducedMotion
{
    private static bool _isActive;

    /// <summary>True when reduced motion is in effect. Components branch on this.</summary>
    public static bool IsActive => _isActive;

    /// <summary>Raised whenever <see cref="IsActive"/> flips value.</summary>
    public static event EventHandler<bool>? Changed;

    /// <summary>Enable reduced motion. Idempotent.</summary>
    public static void Enable()
    {
        if (_isActive) return;
        _isActive = true;
        Changed?.Invoke(null, true);
    }

    /// <summary>Disable reduced motion. Idempotent.</summary>
    public static void Disable()
    {
        if (!_isActive) return;
        _isActive = false;
        Changed?.Invoke(null, false);
    }
}
