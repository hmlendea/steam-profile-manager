using System;

namespace SteamProfileManager.Models
{
    public class SteamUser
    {
        public SteamUserId SteamId { get; set; }

        public string Name { get; set; }

        public bool IsOnline { get; set; }

        public bool IsScammer { get; set; }

        public DateTime LastOnlineDate { get; set; }
    }
}
