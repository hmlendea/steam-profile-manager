using System;

using NuciLog.Core;
using NuciWeb.Steam;

using SteamProfileManager.Configuration;

namespace SteamProfileManager.Service
{
    public sealed class ProfileManager : IProfileManager
    {
        readonly IInfoGenerator infoGenerator;
        readonly ISteamProcessor steamProcessor;
        readonly BotSettings botSettings;
        readonly ILogger logger;

        public ProfileManager(
            IInfoGenerator infoGenerator,
            ISteamProcessor steamProcessor,
            BotSettings botSettings,
            ILogger logger)
        {
            this.infoGenerator = infoGenerator;
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
            string username = infoGenerator.GetRandomUsername();
            throw new NotImplementedException();
        }
    }
}
