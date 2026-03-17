using CertMonitor.API.Interfaces;
using CertMonitor.API.Models;
using System.Text;
using System.Text.Json;

namespace CertMonitor.API.Services;

public sealed class CertificateService : ICertificateService
{
    private List<Certificate> _certificates = new();
    private readonly string _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates.json");

    public event Action<string>? OnDataProcessed;

    public void ProcessIncomingData(Guid id, object rawData)
    {
        var cert = this._certificates.FirstOrDefault(c => c.Id == id);
        if (cert is null) return;

        if (rawData is DateOnly newExpiry)
        {
            cert.RenewCertificate(newExpiry);
        }
        else if (rawData is string textData)
        {
            var cleanNote = textData.Trim();
            if (cleanNote.Length > 0)
            {
                cert.UpdateMetadata(cleanNote);
            }
        }
        else if (rawData is JsonElement { ValueKind: JsonValueKind.Number } jsonNum)
        {
            int days = jsonNum.GetInt32();
            cert.RenewCertificate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days)));
        }

        this.OnDataProcessed?.Invoke($"Zaktualizowano certyfikat: {cert.Name}. Obecne dni do wygaśnięcia: {cert.ToExpire}");
    }

    public string ExportToCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nazwa;Typ;DniDoWygasniecia;Status");

        if (this._certificates is []) return ""; 

        foreach (var cert in this._certificates)
        {
            string statusDescription = cert.ToExpire switch
            {
                < 0 => "Wygasł!",
                < 7 => "Krytyczny (Mniej niż tydzień)",
                < 30 => "Ostrzeżenie (Miesiąc)",
                _ => "Bezpieczny"
            };

            sb.AppendLine($"{cert.Name};{cert.Type};{cert.ToExpire};{statusDescription}");
        }

        return sb.ToString();
    }

    public Task<List<Certificate>> GetCertificatesAsync()
    {
        if (this._certificates.Any())
            return Task.FromResult(this._certificates);

        if (!File.Exists(this._jsonPath))
            throw new FileNotFoundException("File does not exists");

        string json = File.ReadAllText(this._jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        this._certificates = JsonSerializer.Deserialize<List<Certificate>>(json, options) ?? new();

        return Task.FromResult(this._certificates);
    }
}
