using System;

namespace SteamProfileManager.Models.ProfileTriggers
{
    public class SpecificDateProfileTrigger : ProfileTrigger
    {
        public override Type Type => typeof(SpecificDateProfileTrigger);

        public override DateTime LastRun { get; set; }

        public DateTime Date { get; set; }

        public override bool CanRun => DateTime.Now >= Date && LastRun < Date;
    }
}
