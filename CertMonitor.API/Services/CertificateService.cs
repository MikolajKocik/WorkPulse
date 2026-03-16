using CertMonitor.API.Interfaces;
using CertMonitor.API.Models;
using System.Text;

namespace CertMonitor.API.Services;

public sealed class CertificateService : ICertificateService
{
    private readonly List<Certificate> _certificates = new();

    public void ProcessIncomingData(Guid id, object rawData)
    {
        var cert = this._certificates.FirstOrDefault(c => c.Id == id);
        if (cert is null) return;

        if (rawData is int days)
        {
            cert.RenewCertificate(DateTime.UtcNow.AddDays(days));
            cert.UpdateMetadata(days);
        }
        else if (rawData as string is string textData)
        {
            var cleanNote = textData.Trim();
            if (cleanNote.Length > 0)
            {
                cert.UpdateMetadata(cleanNote);
            }
        }
    }

    public string ExportToCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nazwa;Typ;DataWygasniecia;DniDoKonca;Metadane");

        foreach (var cert in this._certificates)
        {
            string metaDisplay = cert.MetaData switch
            {
                int d => $"Przedłużono o {d} dni",
                string s => $"Notatka: {s}",
                null => "Brak danych",
                _ => "Nieznany format"
            };

            sb.AppendLine($"{cert.Name};{cert.Type};{cert.ExpiredDate:yyyy-MM-dd};{cert.ToExpire};{metaDisplay}");
        }

        return sb.ToString();
    }
}
