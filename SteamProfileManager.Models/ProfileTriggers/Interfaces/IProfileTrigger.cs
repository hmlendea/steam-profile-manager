using System;

namespace SteamProfileManager.Models.ProfileTriggers.Interfaces
{
    public interface IProfileTrigger
    {
        Type Type { get; }

        DateTime LastRun { get; set; }

        bool CanRun { get; }
    }
}
