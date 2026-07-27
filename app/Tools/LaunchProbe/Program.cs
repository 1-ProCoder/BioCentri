using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BioCentri.LaunchProbe;

internal static class Program
{
    [STAThread]
    public static int Main()
    {
        var exePath = @"C:\Users\Princ\BioCentri\app\BioCentri.App\bin\Debug\net8.0-windows10.0.19041.0\BioCentri.App.exe";
        var logPath = @"C:\Users\Princ\BioCentri\app\_runtime_probe.log";

        File.WriteAllText(logPath, "=== BioCentri runtime launch probe ===\nStart: " + DateTime.Now.ToString("O") + "\n");
        Console.WriteLine("Log: " + logPath);

        AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
        {
            try
            {
                var ex = e.Exception;
                var line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] FIRSTCHANCE " + ex.GetType().FullName + ": " + ex.Message;
                if (ex.InnerException != null)
                    line += " | INNER: " + ex.InnerException.GetType().FullName + ": " + ex.InnerException.Message;
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch { }
        };

        if (!File.Exists(exePath))
        {
            File.AppendAllText(logPath, "ABORT: BioCentri.App.exe not found at " + exePath + "\n");
            Console.WriteLine("ABORT: " + exePath);
            return 2;
        }

        File.AppendAllText(logPath, "Launching: " + exePath + "\n");

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };

        Process p;
        try { p = Process.Start(psi); }
        catch (Exception startEx)
        {
            File.AppendAllText(logPath, "Start exception: " + startEx.GetType().FullName + ": " + startEx.Message + "\n");
            Console.WriteLine("Start failed: " + startEx.Message);
            return 3;
        }

        File.AppendAllText(logPath, "Launched PID " + p.Id + "\n");
        Console.WriteLine("Launched PID " + p.Id);

        var probeMs = 12000;
        var sw = Stopwatch.StartNew();
        bool exitedEarly = false;
        while (sw.ElapsedMilliseconds < probeMs)
        {
            if (p.HasExited) { exitedEarly = true; break; }
            Thread.Sleep(250);
        }

        bool alive = !p.HasExited;
        File.AppendAllText(logPath, "Probe end at " + sw.ElapsedMilliseconds + "ms: alive=" + alive + " exitedEarly=" + exitedEarly + (alive ? "" : (" exitCode=" + p.ExitCode)) + "\n");
        Console.WriteLine("Probe: alive=" + alive + " exitedEarly=" + exitedEarly + (alive ? "" : (" exitCode=" + p.ExitCode)));

        if (alive)
        {
            try { p.Kill(); p.WaitForExit(3000); } catch { }
            File.AppendAllText(logPath, "Killed after probe window\n");
            Console.WriteLine("Killed");
        }

        try
        {
            var stdout = p.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(stdout))
                File.AppendAllText(logPath, "--- STDOUT ---\n" + stdout + "\n");
        }
        catch (Exception ex) { File.AppendAllText(logPath, "STDOUT read error: " + ex.Message + "\n"); }

        try
        {
            var stderr = p.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(stderr))
                File.AppendAllText(logPath, "--- STDERR ---\n" + stderr + "\n");
        }
        catch (Exception ex) { File.AppendAllText(logPath, "STDERR read error: " + ex.Message + "\n"); }

        File.AppendAllText(logPath, "End: " + DateTime.Now.ToString("O") + "\n");
        return alive ? 0 : 1;
    }
}
