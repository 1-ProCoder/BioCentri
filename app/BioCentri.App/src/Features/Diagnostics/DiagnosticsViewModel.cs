using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.Diagnostics;

/// <summary>
/// Diagnostics view-model. M2 placeholder per IMPLEMENTATION_PLAN §7.
/// Reports applied in Milestone 5 build the live signal list and the
/// Hello probe; today the page renders addressable environment
/// readouts so the layout is reviewable.
/// </summary>
public sealed partial class DiagnosticsViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Diagnostics).Title;
    public string Subtitle => RouteTable.Get(Route.Diagnostics).Subtitle;

    public ObservableCollection<DiagnosticsRow> Readouts { get; } = new();
    public ObservableCollection<DiagnosticsSignalRow> Signals { get; } = new();

    public DiagnosticsViewModel()
    {
        Readouts.Add(new DiagnosticsRow("Application", "BioCentri.App",         "User-mode WPF"));
        Readouts.Add(new DiagnosticsRow("Hello probe",  "Pending",                "Microsoft.Windows.SDK.NET"));
        Readouts.Add(new DiagnosticsRow("OS",           RuntimeInformation.OSDescription, "User session"));
        Readouts.Add(new DiagnosticsRow("Runtime",      RuntimeInformation.FrameworkDescription, ".NET"));

        Signals.Add(new DiagnosticsSignalRow("INFO", "BioCentri loaded successfully on this thread."));
        Signals.Add(new DiagnosticsSignalRow("WARN", "Hello capability probe is deferred until Milestone 5."));
        Signals.Add(new DiagnosticsSignalRow("INFO", "No outbound network calls observed in this session."));
    }
}

public sealed record DiagnosticsRow(string Label, string Value, string Caption);

public sealed record DiagnosticsSignalRow(string Severity, string Description);
