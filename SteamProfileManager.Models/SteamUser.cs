using System;

namespace SteamProfileManager.Models
{
    public class SteamUser
    {
        public string SteamId { get; set; }

        public int AccountId { get; set; }

        public string Name { get; set; }

        public bool IsOnline { get; set; }

        public bool IsScammer { get; set; }

        public DateTime LastOnlineDate { get; set; }
    }
}
