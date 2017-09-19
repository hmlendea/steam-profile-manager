using System;

namespace SteamProfileManager.Models.ProfileActions
{
    public class ChangeAvatarProfileAction : ProfileAction
    {
        public override Type Type => typeof(ChangeAvatarProfileAction);

        public string FilePath { get; set; }

        public override void Execute()
        {
            // TODO: Do it!
        }
    }
}
