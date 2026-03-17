using System.Security.Claims;

namespace WorkPulse.Interfaces;

public interface IUserContext
{
    public ClaimsPrincipal? User { get; }
}
