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
/// Settings view-model. M7 polish: ships the six settings rows
/// visible on the polished BioCentri Settings page
/// (System Startup &amp; Tray / Challenge Behavior / Appearance),
/// the reduced-motion toggle, and the Windows Hello test affordance.
///
/// Persistence file: <c>%LOCALAPPDATA%\BioCentri\Settings.json</c>.
/// All new properties use <c>init</c> with non-conflicting defaults
/// so legacy <c>Settings.json</c> files (M2..M6) round-trip cleanly
/// through <c>System.Text.Json</c> without throwing.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Settings).Title;
    public string Subtitle => RouteTable.Get(Route.Settings).Subtitle;

    /// <summary>POCO serialised to <c>%LOCALAPPDATA%\BioCentri\Settings.json</c>.
    /// Every property is optional with a sensible default so legacy
    /// files that pre-date a field still deserialise.</summary>
    private sealed record PersistentSettings
    {
        public bool IsReducedMotionEnabled { get; init; }
        public string DefaultAuthMethod { get; init; } = "Biometric";

        // M7 added ----
        public bool IsLaunchOnBootEnabled { get; init; } = true;
        public bool IsMinimizeToTrayEnabled { get; init; } = true;
        public string GracePeriod { get; init; } = "15 mins";
        public string RePromptAfterInactivity { get; init; } = "30 mins";
        public string Theme { get; init; } = "Dark";
        public string AccentColor { get; init; } = "Obsidian Blue";
        // -------------
    }

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BioCentri", "Settings.json");

    private static readonly JsonSerializerOptions SettingsJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IDispatcher _dispatcher;
    private readonly IBiometricAuthService _auth;
    private readonly IToastService _toast;

    // ----------------------------------------------------------
    // Persistence-bound state (driven from PersistentSettings).
    // ----------------------------------------------------------
    private bool _isReducedMotionEnabled;
    private bool _isLaunchOnBootEnabled = true;
    private bool _isMinimizeToTrayEnabled = true;
    private GracePeriodOption _gracePeriod = GracePeriodOption.Mins15;
    private RePromptOption _rePrompt = RePromptOption.Mins30;
    private ThemeOption _theme = ThemeOption.Dark;
    private AccentOption _accent = AccentOption.ObsidianBlue;
    private AuthMethodOption _defaultAuthMethod = AuthMethodOption.Biometric;
    private AuthCapability _authCapability = AuthCapability.Unknown;

    /// <summary>M7: launch-at-startup toggle.</summary>
    public bool IsLaunchOnBootEnabled
    {
        get => _isLaunchOnBootEnabled;
        set { if (SetProperty(ref _isLaunchOnBootEnabled, value)) PersistSettings(); }
    }

    /// <summary>M7: close-to-tray toggle.</summary>
    public bool IsMinimizeToTrayEnabled
    {
        get => _isMinimizeToTrayEnabled;
        set { if (SetProperty(ref _isMinimizeToTrayEnabled, value)) PersistSettings(); }
    }

    public IReadOnlyList<GracePeriodOption> GracePeriodOptions { get; } =
        Enum.GetValues<GracePeriodOption>();

    public GracePeriodOption GracePeriod
    {
        get => _gracePeriod;
        set { if (SetProperty(ref _gracePeriod, value)) PersistSettings(); }
    }

    public IReadOnlyList<RePromptOption> RePromptOptions { get; } =
        Enum.GetValues<RePromptOption>();

    public RePromptOption RePromptAfterInactivity
    {
        get => _rePrompt;
        set { if (SetProperty(ref _rePrompt, value)) PersistSettings(); }
    }

    public IReadOnlyList<ThemeOption> ThemeOptions { get; } =
        Enum.GetValues<ThemeOption>();

    public ThemeOption Theme
    {
        get => _theme;
        set { if (SetProperty(ref _theme, value)) PersistSettings(); }
    }

    public IReadOnlyList<AccentOption> AccentOptions { get; } =
        Enum.GetValues<AccentOption>();

    public AccentOption AccentColor
    {
        get => _accent;
        set { if (SetProperty(ref _accent, value)) PersistSettings(); }
    }

    public bool IsReducedMotionEnabled
    {
        get => _isReducedMotionEnabled;
        set
        {
            if (!SetProperty(ref _isReducedMotionEnabled, value)) return;
            if (value) UseReducedMotion.Enable(); else UseReducedMotion.Disable();
            System.Windows.Application.Current.Resources["Motion.RespectReducedMotion"] = value;
            PersistSettings();
        }
    }

    public AuthMethodOption DefaultAuthMethod
    {
        get => _defaultAuthMethod;
        set { if (SetProperty(ref _defaultAuthMethod, value)) PersistSettings(); }
    }

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

    public string AuthCapabilityLabel => _authCapability switch
    {
        AuthCapability.Available                => "Windows Hello is ready on this device.",
        AuthCapability.NotConfiguredForUser     => "Windows Hello is not yet set up for this user — enroll in Windows Settings → Accounts → Sign-in options.",
        AuthCapability.DisabledByPolicy         => "Windows Hello is blocked by group policy on this device.",
        AuthCapability.NotAvailableForHardware  => "This device has no biometric hardware.",
        _                                       => "Checking biometric capability…",
    };

    public bool CanTestAuth => _authCapability == AuthCapability.Available;

    public IAsyncRelayCommand TestAuthCommand { get; }

    /// <summary>M7: clicked from the Theme picker row in
    /// <see cref="SettingsPage"/>. Bound via
    /// <c>DataContext.SetThemeCommand</c> + CommandParameter.</summary>
    [RelayCommand]
    private void SetTheme(ThemeOption option)
    {
        Theme = option;
    }

    /// <summary>M7: clicked from the Accent Color picker row in
    /// <see cref="SettingsPage"/>.</summary>
    [RelayCommand]
    private void SetAccentColor(AccentOption option)
    {
        AccentColor = option;
    }

    public ObservableCollection<SettingsCategoryRow> Categories { get; } = new();

    public SettingsViewModel(IBiometricAuthService auth, IToastService toast, IDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(auth);
        ArgumentNullException.ThrowIfNull(toast);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _auth = auth;
        _toast = toast;
        _dispatcher = dispatcher;

        // Sidebar list — kept for navigation parity with prior milestones.
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

    private void FireAndForgetCapabilityProbe() => _ = ProbeAuthCapabilityAsync();

    private async Task ProbeAuthCapabilityAsync()
    {
        try
        {
            var cap = await _auth.GetCapabilityAsync(CancellationToken.None).ConfigureAwait(false);
            await _dispatcher.InvokeAsync(() => AuthCapability = cap).ConfigureAwait(false);
        }
        catch { /* see SettingsViewModel docstring */ }
    }

    private async Task TestAuthAsync()
    {
        const string testAppName = "BioCentri · Auth test";
        var outcome = await _auth.AuthenticateAsync(testAppName, CancellationToken.None).ConfigureAwait(false);
        await _dispatcher.InvokeAsync(() => ShowOutcomeToast(outcome)).ConfigureAwait(false);
    }

    private void ShowOutcomeToast(AuthOutcome outcome)
    {
        switch (outcome)
        {
            case AuthOutcome.Verified:
                _toast.Show(ToastSeverity.Success, "Hello verified",
                    "Windows Hello confirmed your identity."); break;
            case AuthOutcome.UserCancelled:
                _toast.Show(ToastSeverity.Info, "Auth test dismissed",
                    "You closed the Windows Hello prompt."); break;
            case AuthOutcome.NotConfiguredForUser:
                _toast.Show(ToastSeverity.Warning, "Hello not set up",
                    "Enroll a fingerprint, face, or PIN in Windows Settings → Accounts → Sign-in options."); break;
            case AuthOutcome.DisabledByPolicy:
                _toast.Show(ToastSeverity.Warning, "Hello blocked by policy",
                    "Your organisation has disabled biometric authentication on this device."); break;
            case AuthOutcome.DeviceUnavailable:
                _toast.Show(ToastSeverity.Warning, "Biometric unavailable",
                    "The biometric device is busy or temporarily unavailable."); break;
            case AuthOutcome.RetriesExhausted:
                _toast.Show(ToastSeverity.Danger, "Auth failed",
                    "Windows Hello could not verify your identity after the OS retry window."); break;
            case AuthOutcome.Deduped: break;
            default:
                _toast.Show(ToastSeverity.Warning, "Auth test",
                    $"Outcome: {outcome}"); break;
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
                _isReducedMotionEnabled   = settings.IsReducedMotionEnabled;
                _isLaunchOnBootEnabled    = settings.IsLaunchOnBootEnabled;
                _isMinimizeToTrayEnabled  = settings.IsMinimizeToTrayEnabled;
                _gracePeriod              = ParseGrace(settings.GracePeriod);
                _rePrompt                 = ParseRePrompt(settings.RePromptAfterInactivity);
                _theme                    = ParseTheme(settings.Theme);
                _accent                   = ParseAccent(settings.AccentColor);
                _defaultAuthMethod        = ParseAuthMethod(settings.DefaultAuthMethod);

                OnPropertyChanged(nameof(IsReducedMotionEnabled));
                OnPropertyChanged(nameof(IsLaunchOnBootEnabled));
                OnPropertyChanged(nameof(IsMinimizeToTrayEnabled));
                OnPropertyChanged(nameof(GracePeriod));
                OnPropertyChanged(nameof(RePromptAfterInactivity));
                OnPropertyChanged(nameof(Theme));
                OnPropertyChanged(nameof(AccentColor));
                OnPropertyChanged(nameof(DefaultAuthMethod));

                if (_isReducedMotionEnabled) UseReducedMotion.Enable();
                System.Windows.Application.Current.Resources["Motion.RespectReducedMotion"]
                    = _isReducedMotionEnabled;
            }
        }
        catch { /* first launch / corrupt */ }
    }

    private void PersistSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var settings = new PersistentSettings
            {
                IsReducedMotionEnabled   = _isReducedMotionEnabled,
                IsLaunchOnBootEnabled    = _isLaunchOnBootEnabled,
                IsMinimizeToTrayEnabled  = _isMinimizeToTrayEnabled,
                GracePeriod              = _gracePeriod.ToString(),
                RePromptAfterInactivity  = _rePrompt.ToString(),
                Theme                    = _theme.ToString(),
                AccentColor              = _accent.ToString(),
                DefaultAuthMethod        = _defaultAuthMethod.ToString(),
            };
            var json = JsonSerializer.Serialize(settings, SettingsJsonOptions);
            var temp = SettingsPath + ".tmp";
            File.WriteAllText(temp, json);
            File.Move(temp, SettingsPath, overwrite: true);
        }
        catch { /* best-effort */ }
    }

    private static GracePeriodOption ParseGrace(string? raw)
        => Enum.TryParse<GracePeriodOption>(raw, ignoreCase: true, out var v) ? v : GracePeriodOption.Mins15;
    private static RePromptOption ParseRePrompt(string? raw)
        => Enum.TryParse<RePromptOption>(raw, ignoreCase: true, out var v) ? v : RePromptOption.Mins30;
    private static ThemeOption ParseTheme(string? raw)
        => Enum.TryParse<ThemeOption>(raw, ignoreCase: true, out var v) ? v : ThemeOption.Dark;
    private static AccentOption ParseAccent(string? raw)
        => Enum.TryParse<AccentOption>(raw, ignoreCase: true, out var v) ? v : AccentOption.ObsidianBlue;
    private static AuthMethodOption ParseAuthMethod(string? raw)
        => Enum.TryParse<AuthMethodOption>(raw, ignoreCase: true, out var v) ? v : AuthMethodOption.Biometric;
}

public enum GracePeriodOption { Mins5, Mins15, Hr1 }
public enum RePromptOption   { Mins10, Mins30, Never }
public enum ThemeOption      { Dark, Light, System }
public enum AccentOption     { ObsidianBlue, EmeraldGreen, AmethystPurple }
public enum AuthMethodOption { Biometric, PinFallback }

public sealed record SettingsCategoryRow(string Title, string Subtitle, string Glyph);
