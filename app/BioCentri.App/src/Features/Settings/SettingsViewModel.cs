using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using BioCentri.App.Hooks;
using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.Settings;

/// <summary>
/// Settings view-model. M2 placeholder per IMPLEMENTATION_PLAN §7.
/// FR-6 detail panel arrives in Milestone 6; today the view-model
/// exposes the category list so navigation + design language work
/// end-to-end.
/// </summary>
public sealed class SettingsViewModel : ObservableObject
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

    public ObservableCollection<SettingsCategoryRow> Categories { get; } = new();

    public SettingsViewModel()
    {
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
