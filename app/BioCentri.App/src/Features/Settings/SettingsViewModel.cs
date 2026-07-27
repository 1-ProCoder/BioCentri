using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BioCentri.App.Hooks;
using BioCentri.App.Routing;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.Settings;

/// <summary>
/// Settings view-model. M2 placeholder per IMPLEMENTATION_PLAN §7.
/// FR-6 detail panel arrives in Milestone 6; today the view-model
/// exposes the category list so navigation + design language work
/// end-to-end.
///
/// M7.5+: visibly demonstrates the Windows Hello pipeline so the user
/// can confirm auth works without first configuring a protected app.
/// Injects <see cref="IBiometricAuthService"/> + <see cref="IToastService"/>
/// + <see cref="IDispatcher"/>; surfaces live capability + a Test command.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Settings).Title;
    public string Subtitle => RouteTable.Get(Route.Settings).Subtitle;

    /// <summary>POCO serialised to <c>%LOCALAPPDATA%\BioCentri\Settings.json</c>
    /// so user preferences survive app restarts. <see cref="DefaultAuthMethod"/>
    /// is the fallback credential tier the OS offers when biometric is
    /// disabled by policy / not configured for this user.</summary>
    private sealed record PersistentSettings(
        bool IsReducedMotionEnabled,
        string DefaultAuthMethod);

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BioCentri",
        "Settings.json");

    private static readonly JsonSerializerOptions SettingsJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IDispatcher _dispatcher;
    private readonly IBiometricAuthService _auth;
    private readonly IToastService _toast;

    private bool _isReducedMotionEnabled;

    /// <summary>
    /// Milestone 7: reduced-motion toggle. Mirrored into
    /// <c>UseReducedMotion</c> and the <c>Motion.RespectReducedMotion</c>
    /// Application resource so all animation components gate on it.
    /// Persisted to disk on every change; loaded on construction.
    /// </summary>
    public bool IsReducedMotionEnabled
    {
        get => _isReducedMotionEnabled;
        set
        {
            if (!SetProperty(ref _isReducedMotionEnabled, value)) return;

            if (value)
                UseReducedMotion.Enable();
            else
                UseReducedMotion.Disable();

            System.Windows.Application.Current.Resources["Motion.RespectReducedMotion"] = value;

            PersistSettings();
        }
    }

    private AuthMethodOption _defaultAuthMethod = AuthMethodOption.Biometric;

    /// <summary>
    /// Fallback credential choice when Windows Hello biometric is
    /// unavailable or disabled by policy. <c>Biometric</c> prefers
    /// fingerprint / face when available; <c>PinFallback</c> routes the
    /// user through the OS PIN sign-in path instead. Persisted.
    /// </summary>
    public AuthMethodOption DefaultAuthMethod
    {
        get => _defaultAuthMethod;
        set
        {
            if (!SetProperty(ref _defaultAuthMethod, value)) return;
            PersistSettings();
        }
    }

    /// <summary>
    /// M7.5: current device capability reported by the biometric
    /// adapter. Surfaces a "Windows Hello not configured" hint on the
    /// Settings page so the user knows what to do if the Test button
    /// reports a non-Verified outcome. The probe runs once at boot;
    /// <see cref="ILocalJsonStore"/> does NOT persist this — it's a
    /// runtime-only signal of what the OS will actually accept.
    /// </summary>
    private AuthCapability _authCapability = AuthCapability.Unknown;

    public AuthCapability AuthCapability
    {
        get => _authCapability;
        private set
        {
            if (!SetProperty(ref _authCapability, value)) return;
            OnPropertyChanged(nameof(AuthCapabilityLabel));
            OnPropertyChanged(nameof(CanTestAuth));
            TestAuthCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>Single-line status copy rendered on the Settings page.</summary>
    public string AuthCapabilityLabel => _authCapability switch
    {
        AuthCapability.Available                => "Windows Hello is ready on this device.",
        AuthCapability.NotConfiguredForUser     => "Windows Hello is not yet set up for this user — enroll in Windows Settings → Accounts → Sign-in options.",
        AuthCapability.DisabledByPolicy         => "Windows Hello is blocked by group policy on this device.",
        AuthCapability.NotAvailableForHardware  => "This device has no biometric hardware.",
        _                                       => "Checking biometric capability…",
    };

    public bool CanTestAuth => _authCapability == AuthCapability.Available;

    /// <summary>
    /// M7.5: Test button on the Settings page. Prompts the OS for the
    /// fake app name "BioCentri · Auth test" and toasts the outcome so
    /// the user can SEE the full Windows Hello pipeline without first
    /// configuring a protected app launch.
    /// </summary>
    public IAsyncRelayCommand TestAuthCommand { get; }

    public ObservableCollection<SettingsCategoryRow> Categories { get; } = new();

    public SettingsViewModel(IBiometricAuthService auth, IToastService toast, IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(auth);
        ArgumentNullException.ThrowIfNull(toast);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _auth = auth;
        _toast = toast;
        _dispatcher = dispatcher;

        // IconKey map: only keys that exist in app/BioCentri.App/src/styles/Icons.xaml
        // (Decision 9 followup: action keys are limited to what the design system ships.)
        Categories.Add(new SettingsCategoryRow(
            "Appearance",   "Theme, density, motion",                "Icons.Action.More"));
        Categories.Add(new SettingsCategoryRow(
            "Accessibility","High-contrast, reduced motion, fonts",  "Icons.Status.Info"));
        Categories.Add(new SettingsCategoryRow(
            "Startup",      "Launch with Windows, tray behaviour",  "Icons.Action.Menu"));
        Categories.Add(new SettingsCategoryRow(
            "Notifications","Toasts and the activity feed",         "Icons.Route.Activity"));
        Categories.Add(new SettingsCategoryRow(
            "Privacy",      "Local-only data, deletion",             "Icons.Status.Protected"));
        Categories.Add(new SettingsCategoryRow(
            "About",        "Version, build, credits",               "Icons.Route.About"));

        LoadSettings();

        TestAuthCommand = new AsyncRelayCommand(
            execute: TestAuthAsync,
            canExecute: () => _authCapability == AuthCapability.Available);

        FireAndForgetCapabilityProbe();
    }

    private void FireAndForgetCapabilityProbe()
    {
        // Fire-and-forget; outcome updates AuthCapability on the dispatcher.
        // Exceptions stay swallowed because the UI surfaces "Checking…"
        // as the default and a transient probe failure should not crash
        // the shell.
        _ = ProbeAuthCapabilityAsync();
    }

    private async Task ProbeAuthCapabilityAsync()
    {
        try
        {
            var cap = await _auth.GetCapabilityAsync(CancellationToken.None).ConfigureAwait(false);
            await _dispatcher.InvokeAsync(() => AuthCapability = cap).ConfigureAwait(false);
        }
        catch
        {
            // Leave default (Unknown). UI shows "Checking…"; user can
            // hit Test to force a fresh prompt if they want.
        }
    }

    private async Task TestAuthAsync()
    {
        // Use a stable, recognisable app name so the OS prompt shows
        // "Verify your identity to launch BioCentri · Auth test" and
        // any coalesced waiting watcher-side would land here too.
        const string testAppName = "BioCentri · Auth test";
        var outcome = await _auth.AuthenticateAsync(testAppName, CancellationToken.None).ConfigureAwait(false);

        // ToastService mutations and ItemsControl binding notify must
        // marshal back onto the WPF UI thread.
        await _dispatcher.InvokeAsync(() => ShowOutcomeToast(outcome)).ConfigureAwait(false);
    }

    private void ShowOutcomeToast(AuthOutcome outcome)
    {
        switch (outcome)
        {
            case AuthOutcome.Verified:
                _toast.Show(
                    ToastSeverity.Success,
                    "Hello verified",
                    "Windows Hello confirmed your identity.");
                break;
            case AuthOutcome.UserCancelled:
                _toast.Show(
                    ToastSeverity.Info,
                    "Auth test dismissed",
                    "You closed the Windows Hello prompt.");
                break;
            case AuthOutcome.NotConfiguredForUser:
                _toast.Show(
                    ToastSeverity.Warning,
                    "Hello not set up",
                    "Enroll a fingerprint, face, or PIN in Windows Settings → Accounts → Sign-in options.");
                break;
            case AuthOutcome.DisabledByPolicy:
                _toast.Show(
                    ToastSeverity.Warning,
                    "Hello blocked by policy",
                    "Your organisation has disabled biometric authentication on this device.");
                break;
            case AuthOutcome.DeviceUnavailable:
                _toast.Show(
                    ToastSeverity.Warning,
                    "Biometric unavailable",
                    "The biometric device is busy or temporarily unavailable.");
                break;
            case AuthOutcome.RetriesExhausted:
                _toast.Show(
                    ToastSeverity.Danger,
                    "Auth failed",
                    "Windows Hello could not verify your identity after the OS retry window.");
                break;
            case AuthOutcome.Deduped:
                // 500ms window swallowed the call because another prompt
                // just ran. Don't surface a toast — user didn't see it.
                break;
            default:
                _toast.Show(
                    ToastSeverity.Warning,
                    "Auth test",
                    $"Outcome: {outcome}");
                break;
        }
    }

    private void LoadSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            if (!File.Exists(SettingsPath)) return;

            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<PersistentSettings>(json, SettingsJsonOptions);
            if (settings is not null)
            {
                _isReducedMotionEnabled = settings.IsReducedMotionEnabled;
                _defaultAuthMethod = ParseAuthMethod(settings.DefaultAuthMethod);
                OnPropertyChanged(nameof(IsReducedMotionEnabled));
                OnPropertyChanged(nameof(DefaultAuthMethod));

                if (_isReducedMotionEnabled)
                    UseReducedMotion.Enable();

                System.Windows.Application.Current.Resources["Motion.RespectReducedMotion"]
                    = _isReducedMotionEnabled;
            }
        }
        catch
        {
            // First launch, corrupt file, or permission issue — use defaults.
        }
    }

    private void PersistSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var settings = new PersistentSettings(
                IsReducedMotionEnabled: _isReducedMotionEnabled,
                DefaultAuthMethod:    _defaultAuthMethod.ToString());
            var json = JsonSerializer.Serialize(settings, SettingsJsonOptions);

            // Atomic write (temp + rename) — same durability as LocalJsonStore.
            var temp = SettingsPath + ".tmp";
            File.WriteAllText(temp, json);
            File.Move(temp, SettingsPath, overwrite: true);
        }
        catch
        {
            // Best-effort persistence; don't crash the setter.
        }
    }

    private static AuthMethodOption ParseAuthMethod(string? raw)
    {
        if (Enum.TryParse<AuthMethodOption>(raw, ignoreCase: true, out var parsed))
            return parsed;
        return AuthMethodOption.Biometric;
    }
}

/// <summary>
/// Public UI-facing enum bound by the Settings page. Maps to the
/// app-internal <c>AuthOutcome</c> / OS choice; kept as a stable
/// 2-value type today because Phase-2 will add the broader
/// time-window policies on top.
/// </summary>
public enum AuthMethodOption
{
    Biometric,
    PinFallback,
}

public sealed record SettingsCategoryRow(string Title, string Subtitle, string Glyph);
