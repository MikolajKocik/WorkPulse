using CertMonitor.API.Interfaces;
using WorkPulse.Interfaces;

namespace CertMonitor.API.Jobs;

public sealed class CertificateWatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CertificateWatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var certService = scope.ServiceProvider.GetRequiredService<ICertificateService>();
                var systemService = scope.ServiceProvider.GetRequiredService<ISystemService>();

                var certs = await certService.GetCertificatesAsync();
                int criticalCount = certs.Count(c => c.ToExpire < 7 && c.IsActive);

                if (criticalCount > 0)
                {
                    await systemService.RunHealthAsync();
                }
            } 

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}