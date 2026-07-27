using System.Collections.ObjectModel;
using System.Reflection;
using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.About;

/// <summary>
/// About page view-model. Milestone 5: Version + BuildLabel + License
/// are read from the entry assembly metadata (csproj &lt;Version&gt;,
/// &lt;InformationalVersion&gt;, &lt;Copyright&gt;) so they stay in
/// lock-step with the shipped binary — no hardcoded strings to drift.
/// </summary>
public sealed partial class AboutViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.About).Title;
    public string Subtitle => RouteTable.Get(Route.About).Subtitle;

    public string Version { get; }
    public string BuildLabel { get; }
    public string LicenseName { get; }
    public string BrandLine { get; } = "BioCentri";
    public string BrandTagline { get; } = "Local-first Windows Hello made visible.";

    public ObservableCollection<AboutStatRow> Stats { get; } = new();
    public ObservableCollection<AboutLinkRow> Links { get; } = new();
    public ObservableCollection<AboutRoadmapRow> Roadmap { get; } = new();

    public string CreditsLine =>
        "BioCentri is built by its team and contributors. The design language is shared with the marketing site in /website/.";

    public AboutViewModel()
    {
        var asm = typeof(AboutViewModel).Assembly;
        var asmName = asm.GetName();
        Version = asmName.Version?.ToString(3) ?? "0.0.0";
        BuildLabel = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        LicenseName = asm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "All rights reserved";

        Stats.Add(new AboutStatRow("Version", Version, "Pre-release"));
        Stats.Add(new AboutStatRow("Build", BuildLabel, "Development"));
        Stats.Add(new AboutStatRow("License", LicenseName, "© BioCentri team"));

        Links.Add(new AboutLinkRow("GitHub",   "Source · Releases · Issues", "Icons.Route.Diagnostics", "https://github.com/biocentri/biocentri"));
        Links.Add(new AboutLinkRow("Website",  "Marketing site · Manifesto", "Icons.Shell.Logo",         "https://biocentri.app"));
        Links.Add(new AboutLinkRow("Roadmap",  "What we are shipping next",  "Icons.Route.Activity",    "https://biocentri.app/roadmap"));

        Roadmap.Add(new AboutRoadmapRow("M5 — Real functionality",       "Pages bind to local JSON state; settings, protected apps, rules, and activity all persist between launches."));
        Roadmap.Add(new AboutRoadmapRow("M6 — Real process monitoring",  "Win32_ProcessStartTrace watcher replaces StubProcessMonitor; Windows Hello challenges fire on protected launches."));
        Roadmap.Add(new AboutRoadmapRow("M7 — Polish + Installer",       "WiX 3.14 installer + code signing for SmartScreen trust."));
    }
}

public sealed record AboutStatRow(string Label, string Value, string Caption);

public sealed record AboutLinkRow(string Title, string Description, string IconKey, string Url);

public sealed record AboutRoadmapRow(string Milestone, string Description);
