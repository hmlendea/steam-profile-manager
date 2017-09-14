using SteamProfileManager.Models.ProfileActions.Interfaces;
using SteamProfileManager.Models.ProfileTriggers.Interfaces;

namespace SteamProfileManager.Models
{
    public class ProfileEvent
    {
        public string Id { get; set; }

        public IProfileAction Action { get; set; }

        public IProfileTrigger Trigger { get; set; }
    }
}
