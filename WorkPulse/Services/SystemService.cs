using System.Diagnostics;
using WorkPulse.Interfaces;

namespace WorkPulse.Services;

public sealed class SystemService : ISystemService
{
    public event Action<string>? OnSystemAlert;

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
