using System;

using SteamProfileManager.Models.ProfileActions.Interfaces;

namespace SteamProfileManager.Models.ProfileActions
{
    public class ChangeAvatarProfileAction : IProfileAction
    {
        public Type Type => typeof(ChangeAvatarProfileAction);

        public string FilePath { get; set; }

        public void Execute()
        {

        }
    }
}
