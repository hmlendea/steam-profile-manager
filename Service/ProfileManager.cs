using System;

using NuciLog.Core;
using NuciWeb.Steam;

using SteamProfileManager.Configuration;

namespace SteamProfileManager.Service
{
    public sealed class ProfileManager : IProfileManager
    {
        readonly ISteamProcessor steamProcessor;
        readonly BotSettings botSettings;
        readonly ILogger logger;

        public ProfileManager(
            ISteamProcessor steamProcessor,
            BotSettings botSettings,
            ILogger logger)
        {
            this.steamProcessor = steamProcessor;
            this.botSettings = botSettings;
            this.logger = logger;
        }

        public void LogIn()
        {
            steamProcessor.LogIn(botSettings.SteamAccount);
        }

        public void SetRandomUsername()
        {
            throw new NotImplementedException();
        }
    }
}
