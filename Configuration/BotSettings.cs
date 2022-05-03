using NuciWeb.Steam.Models;

namespace SteamProfileManager.Configuration
{
    public sealed class BotSettings
    {
        public int PageLoadTimeout { get; set; }

        public SteamAccount SteamAccount { get; set; }

        public bool RandomiseProfileName { get; set; }

        public string ProfileNamesList { get; set; }
    }
}
