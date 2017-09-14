using SteamProfileManager.Models.ProfileActions.Interfaces;
using SteamProfileManager.Models.ProfileTriggers.Interfaces;

namespace SteamProfileManager.Models
{
    public class ProfileEvent
    {
        IProfileAction Action { get; set; }

        IProfileTrigger Trigger { get; set; }
    }
}
