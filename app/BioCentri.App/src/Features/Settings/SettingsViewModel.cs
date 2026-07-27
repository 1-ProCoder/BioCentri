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
    /// so the reduced-motion toggle survives app restarts.</summary>
    private sealed record PersistentSettings(bool IsReducedMotionEnabled);

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
                OnPropertyChanged(nameof(IsReducedMotionEnabled));

                if (_isReducedMotionEnabled)
                    UseReducedMotion.Enable();

                System.Windows.Application.Current.Resources["Motion.RespectReducedMotion"]
                    = _isReducedMotionEnabled;
            }
        }
        catch
        {
            // First launch, corrupt file, or permission issue — use default (false).
        }
    }

    private void PersistSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            var settings = new PersistentSettings(_isReducedMotionEnabled);
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
}

public sealed record SettingsCategoryRow(string Title, string Subtitle, string Glyph);
