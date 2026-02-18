using System.Threading;
using System.Threading.Tasks;
using Azure.API.Models.Profiles;

namespace Azure.API.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileList> GetUserProfilesAsync(CancellationToken continuationToken = default);
    }
}
