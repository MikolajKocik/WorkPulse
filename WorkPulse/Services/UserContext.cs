using System.Security.Claims;
using WorkPulse.Services.Interfaces;

namespace WorkPulse.Services;

public sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor context;

    public UserContext(IHttpContextAccessor context)
    {
        this.context = context;
    }

    public ClaimsPrincipal User => this.context.HttpContext?.User;
}
