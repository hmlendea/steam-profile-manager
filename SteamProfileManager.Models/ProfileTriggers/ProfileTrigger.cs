using System;

namespace SteamProfileManager.Models.ProfileTriggers
{
    public abstract class ProfileTrigger
    {
        public abstract Type Type { get; }

        public abstract DateTime LastRun { get; set; }

        public abstract bool CanRun { get; }

        public ProfileTrigger()
        {
            LastRun = new DateTime(0);
        }
    }
}
