namespace BioCentri.App.Types.Services;

/// <summary>
/// Lookup for "is this process protected?" — the persistence-backed
/// list of apps the user has elected to gate with biometrics. The
/// interface is intentionally tiny so a Milestone-6 watcher can swap
/// in a file-backed store without touching the watcher pipeline.
/// </summary>
public interface IAuthAppRules
{
    /// <summary>
    /// Returns true when <paramref name="processName"/> should trigger
    /// a biometric challenge before launch. Match semantics are
    /// case-insensitive and tolerate either bare exe names
    /// ("chrome.exe") or full paths ("C:\Program Files\...").
    /// </summary>
    bool IsProtected(string processName);
}
