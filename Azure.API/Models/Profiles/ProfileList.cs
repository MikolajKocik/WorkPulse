using Azure.API.Models.Profiles;

namespace Azure.API.Models.Profiles;

public class ProfileList
{
    public List<Profile> Profiles { get; set; } = new();
}