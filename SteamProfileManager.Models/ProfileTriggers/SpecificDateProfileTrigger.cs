using System;

using SteamProfileManager.Models.ProfileTriggers.Interfaces;

namespace SteamProfileManager.Models.ProfileTriggers
{
    public class SpecificDateProfileTrigger : IProfileTrigger
    {
        public Type Type => typeof(SpecificDateProfileTrigger);

        public DateTime LastRun { get; set; }

        public DateTime Date { get; set; }

        public bool CanRun => DateTime.Now >= Date && LastRun < Date;

        public SpecificDateProfileTrigger()
        {
            LastRun = new DateTime(0);
        }
    }
}
