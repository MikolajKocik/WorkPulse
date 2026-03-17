using CertMonitor.API.Models;

namespace CertMonitor.API.Interfaces;

public interface ICertificateService
{
    event Action<string>? OnDataProcessed;
    void ProcessIncomingData(Guid id, object rawData);
    string ExportToCsv();
    Task<List<Certificate>> GetCertificatesAsync();
}
