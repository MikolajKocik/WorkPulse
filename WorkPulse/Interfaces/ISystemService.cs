namespace WorkPulse.Interfaces;

public interface ISystemService
{
    event Action<string>? OnSystemAlert;
    Task RunHealthAsync();
}
