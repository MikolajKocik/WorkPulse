using System.Security.Claims;

namespace WorkPulse.Services.Interfaces;

public interface IUserContext
{
    public ClaimsPrincipal User { get; }
}
