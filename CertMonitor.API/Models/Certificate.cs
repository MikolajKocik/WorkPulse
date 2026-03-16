using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CertMonitor.API.Models;

public sealed class Certificate 
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; } 
    public CertificationType Type { get; set; }
    public DateTime ExpiredDate { get; private set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; private set; }
    public int ToExpire => (this.ExpiredDate - DateTime.UtcNow).Days;
    public object? MetaData { get; private set; }
    
    public void UpdateMetadata(object data) => this.MetaData = data;

    public void DeactivateCertification()
    {
        this.IsActive = false;
    }

    public void RenewCertificate(DateTime date)
    {
        if (date < DateTime.UtcNow)
            throw new ArgumentException("Date cant be in the past.");

        this.ExpiredDate = date;
    }
}