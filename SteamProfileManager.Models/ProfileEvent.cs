using SteamProfileManager.Models.ProfileActions;
using SteamProfileManager.Models.ProfileTriggers;

namespace SteamProfileManager.Models
{
    public class ProfileEvent
    {
        public string Id { get; set; }

        public ProfileAction Action { get; set; }

        public ProfileTrigger Trigger { get; set; }
    }
}
