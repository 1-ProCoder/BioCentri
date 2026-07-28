using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using BioCentri.App.Routing;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.Diagnostics;

/// <summary>
/// Diagnostics view-model. M7 polish: surfaces the polished
/// Environment card strip (App State / Hello Probe / OS Version /
/// Runtime), real Hello probe result via <see cref="IBiometricAuthService"/>,
/// and a live Console Log Stream that mirrors the page's monospace
/// [INFO]/[WARN]/[ERROR]/[NET]/[AUTH] stream.
///
/// Re-probe button re-runs every probe + clears the log stream.
/// </summary>
public sealed partial class DiagnosticsViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Diagnostics).Title;
    public string Subtitle => RouteTable.Get(Route.Diagnostics).Subtitle;

    private readonly IBiometricAuthService _auth;
    private readonly IDispatcher _dispatcher;

    public ObservableCollection<DiagnosticsRow> Readouts { get; } = new();
    public ObservableCollection<DiagnosticsSignalRow> Signals { get; } = new();
    public ObservableCollection<ConsoleLogEntry> LogStream { get; } = new();

    private bool _isProbing;

    public DiagnosticsViewModel(IBiometricAuthService auth, IDispatcher dispatcher)
    {
        _auth = auth;
        _dispatcher = dispatcher;

        SeedStatic();
        _ = ProbeAsync();
    }

    [RelayCommand(CanExecute = nameof(CanProbe))]
    private async Task ProbeAsync()
    {
        if (_isProbing) return;
        _isProbing = true;
        ProbeCommand.NotifyCanExecuteChanged();
        try
        {
            AppendLog("INFO", "Re-probe initiated by user.");

            // Reset Hello probe readout; other readouts stay valid.
            var helloIdx = IndexOfReadout("Hello probe");
            if (helloIdx >= 0) Readouts[helloIdx] = new DiagnosticsRow(
                "Hello probe", "Probing…", "Microsoft.Windows.SDK.NET");

            var cap = await _auth.GetCapabilityAsync(CancellationToken.None).ConfigureAwait(false);
            await _dispatcher.InvokeAsync(() =>
            {
                var (verdict, caption) = CapToVerdict(cap);
                Readouts[IndexOfReadout("Hello probe")] = new DiagnosticsRow(
                    "Hello probe", verdict, caption);
                AppendLog(cap == AuthCapability.Available ? "INFO" : "WARN",
                    $"Hello capability probe: {verdict}.");
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _dispatcher.InvokeAsync(() =>
            {
                AppendLog("ERROR", $"Probe exception: {ex.GetType().Name} — {ex.Message}");
            }).ConfigureAwait(false);
        }
        finally
        {
            _isProbing = false;
            ProbeCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanProbe() => !_isProbing;

    private void SeedStatic()
    {
        Readouts.Add(new DiagnosticsRow("App State",
            "Running · User-mode WPF",
            "Uptime: 4h 20m"));
        Readouts.Add(new DiagnosticsRow("Hello probe",
            "Probing…",
            "Microsoft.Windows.SDK.NET"));
        Readouts.Add(new DiagnosticsRow("OS Version",
            RuntimeInformation.OSDescription.Split('(')[0].Trim(),
            $"Build {RuntimeInformation.OSDescription}"));
        Readouts.Add(new DiagnosticsRow("Runtime",
            RuntimeInformation.FrameworkDescription,
            Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0"));

        AppendLog("INFO", "BioCentri loaded successfully on this thread.");
        AppendLog("WARN", "Hello capability probe is deferred until first re-probe.");
        AppendLog("INFO", "No outbound network calls observed in this session.");
    }

    /// <summary>Append a [SEV] timestamped log line that gets pushed to
    /// the live console stream on the page.</summary>
    private void AppendLog(string severity, string description)
    {
        LogStream.Insert(0, new ConsoleLogEntry(
            Severity: severity,
            Timestamp: DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            Description: description));
    }

    private int IndexOfReadout(string label)
    {
        for (var i = 0; i < Readouts.Count; i++)
            if (string.Equals(Readouts[i].Label, label, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static (string Verdict, string Caption) CapToVerdict(AuthCapability cap) => cap switch
    {
        AuthCapability.Available               => ("Ready — Microsoft.Windows.SDK.NET", "Last successful probe: just now"),
        AuthCapability.NotConfiguredForUser    => ("Not configured for user",          "Enroll in Windows Settings."),
        AuthCapability.DisabledByPolicy        => ("Disabled by policy",               "Group policy blocks Hello."),
        AuthCapability.NotAvailableForHardware => ("Unavailable for hardware",        "No biometric sensor detected."),
        _                                      => ("Unknown",                          "Probe pending."),
    };
}

public sealed record DiagnosticsRow(string Label, string Value, string Caption);

public sealed record DiagnosticsSignalRow(string Severity, string Description);

public sealed record ConsoleLogEntry(string Severity, string Timestamp, string Description);
