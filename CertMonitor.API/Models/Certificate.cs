using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CertMonitor.API.Models;

public sealed class Certificate 
{
    [JsonInclude]
    public DateOnly ExpiredDate { get; private set; }

    [JsonInclude]
    public bool IsActive { get; private set; }

    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; init; } 
    public CertificationType Type { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public int ToExpire => this.ExpiredDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
    public object? MetaData { get; private set; }
    
    public void UpdateMetadata(object data) => this.MetaData = data;

    public void DeactivateCertification()
    {
        this.IsActive = false;
    }

    public void RenewCertificate(DateOnly date)
    {
        if (date < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date cant be in the past.");

        this.ExpiredDate = date;
    }
}