using SteamProfileManager.DataAccess.DataObjects.ProfileActionEntities;
using SteamProfileManager.DataAccess.DataObjects.ProfileTriggerEntities;

namespace SteamProfileManager.DataAccess.DataObjects
{
    public class ProfileEventEntity
    {
        public string Id { get; set; }

        public ProfileActionEntity Action { get; set; }

        public ProfileTriggerEntity Trigger { get; set; }
    }
}
