using NuciWeb.Steam.Models;

namespace SteamProfileManager.Configuration
{
    public sealed class BotSettings
    {
        public int PageLoadTimeout { get; set; }

        public SteamAccount SteamAccount { get; set; }
    }
}
