namespace CertMonitor.API.Models;

public enum CertificateStatus
{
    MoreThanMonth,
    FromWeekToMonth,
    LessThanWeek,
    Expired
}
