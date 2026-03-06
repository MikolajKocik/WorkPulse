using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CertMonitor.API.Models;

public class CertificateAlert
{
    public int Id { get; init; }
    public CertificateStatus Status { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; init; }        

    public Guid CertificateId { get; init; }
    public Certificate? Certificate { get; init; }
}