using System;

namespace SteamProfileManager.Models
{
    public class SteamUser
    {
        /// SHA-1
        public byte[] AvatarHash { get; set; }

        public string Name { get; set; }

        public SteamUserId SteamId { get; set; }

        public bool IsOnline { get; set; }

        public bool IsScammer { get; set; }

        public DateTime LastOnlineDate { get; set; }
    }
}
