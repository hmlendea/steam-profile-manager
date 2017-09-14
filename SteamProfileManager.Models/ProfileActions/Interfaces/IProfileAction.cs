using System;

namespace SteamProfileManager.Models.ProfileActions.Interfaces
{
    public interface IProfileAction
    {
        Type Type { get; }

        void Execute();
    }
}
