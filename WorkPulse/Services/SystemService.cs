using System.Diagnostics;
using WorkPulse.Interfaces;

namespace WorkPulse.Services;

/// <summary>
/// Provides system-level monitoring and diagnostic services for the application, including resource usage reporting,
/// process status checks, and host health diagnostics.
/// </summary>
/// <remarks>The <see cref="SystemService"/> class exposes asynchronous methods to monitor application resources,
/// check for specific running processes, and perform health checks of the host environment. Diagnostic and status
/// reports are delivered via the <see cref="OnSystemAlert"/> event. This class is intended for use in scenarios where
/// real-time system information and alerts are required. All methods execute asynchronously and do not return values
/// directly; results are communicated through event notifications.</remarks>
public sealed class SystemService : ISystemService
{
    public event Action<string>? OnSystemAlert;

    /// <summary>
    /// Monitors the application's current memory usage and uptime, and triggers a system alert with a resource report.
    /// </summary>
    /// <remarks>This method retrieves the application's RAM consumption and the duration since it started,
    /// then formats this information into a report string. The report is sent via the <see cref="OnSystemAlert"/>
    /// event. The method executes asynchronously but does not return a value.</remarks>
    /// <returns></returns>
    public async Task MonitorAppResourcesAsync()
    {
        using var currentProcess = Process.GetCurrentProcess();

        long memoryUsed = currentProcess.WorkingSet64 / 1024 / 1024;

        var upTime = DateTime.Now - currentProcess.StartTime;

        string report = $"[RESOURCE] Zużycie RAM: {memoryUsed} MB | Uptime: {upTime.Hours}h {upTime.Minutes}m";

        OnSystemAlert?.Invoke(report);
    }

    /// <summary>
    /// Checks whether the specified process is currently running on the system asynchronously and raises a system alert
    /// with the result.
    /// </summary>
    /// <remarks>This method searches for processes whose names contain "msedge" (case-insensitive). After
    /// checking, it triggers the <see cref="OnSystemAlert"/> event with a message indicating whether the process is
    /// running.</remarks>
    /// <returns></returns>
    public async Task CheckRunningProcessesAsync()
    {
        Process[] processes = Process.GetProcesses();

        string target = "msedge";
        bool isRunning = processes.Any(p => p.ProcessName.Contains(target, StringComparison.OrdinalIgnoreCase));

        string status = isRunning ? "DZIAŁA" : "NIE URUCHOMIONY";

        this.OnSystemAlert?.Invoke($"[PROCESS] Sprawdzanie: {target} -> Status: {status}");
    }

    /// <summary>
    /// Performs an asynchronous health check of the host system and reports diagnostic information.
    /// </summary>
    /// <remarks>The health check gathers details about the operating system, machine name, and .NET runtime
    /// version. The diagnostic report is sent via the <see cref="OnSystemAlert"/> event. If the .NET CLI is not
    /// available, the report will indicate that the .NET version could not be determined.</remarks>
    /// <returns></returns>
    public async Task RunHealthAsync()
    {
        var sw = Stopwatch.StartNew();

        PlatformID os = Environment.OSVersion.Platform;
        string machine = Environment.MachineName;

        string dotnetInfo = "Brak Danych";
        try
        {
            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            proc.Start();
            dotnetInfo = await proc.StandardOutput.ReadToEndAsync();
        }
        catch
        {
            dotnetInfo = "Dotnet CLI nieodnaleziony";
        }

        sw.Stop();

        string report = $"[DIAG] Host: {machine} | OS: {os} | .NET: {dotnetInfo.Trim()} | Czas: {sw.ElapsedMilliseconds}ms";
        this.OnSystemAlert?.Invoke(report);
    }
}
