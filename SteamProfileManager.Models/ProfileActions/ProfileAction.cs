using System;

namespace SteamProfileManager.Models.ProfileActions
{
    public abstract class ProfileAction
    {
        public abstract Type Type { get; }

        public abstract void Execute();
    }
}
