using System.IO;

using NuciExtensions;
using NuciLog.Core;
using NuciWeb.Steam;

using SteamProfileManager.Configuration;
using SteamProfileManager.Logging;

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
            logger.Debug(
                MyOperation.LogIn,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));

            steamProcessor.LogIn(botSettings.SteamAccount);
            steamProcessor.RejectCookies();

            logger.Info(
                MyOperation.LogIn,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));
        }

        public void SetRandomProfileName()
        {
            logger.Debug(
                MyOperation.SetProfileName,
                OperationStatus.Started);

            string profileName = null;

            if (!string.IsNullOrWhiteSpace(botSettings.ProfileNamesList))
            {
                profileName = GetRandomUsernameFromList();
            }
            else
            {
                profileName = infoGenerator.GetRandomProfileName();
            }

            steamProcessor.SetProfileName(profileName);
            //steamProcessor.SetProfilePicture("/home/horatiu/Downloads/Untitled.png");

            logger.Info(
                MyOperation.SetProfileName,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.ProfileName, profileName));
        }

        public void SetRandomProfileIdentifier()
        {
            logger.Debug(
                MyOperation.SetProfileIdentifier,
                OperationStatus.Started);

            string profileIdentifier = infoGenerator.GetRandomProfileIdentifier();

            steamProcessor.SetProfileIdentifier(profileIdentifier);

            logger.Info(
                MyOperation.SetProfileIdentifier,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.ProfileIdentifier, profileIdentifier));
        }

        private string GetRandomUsernameFromList()
            => File
                .ReadAllLines(botSettings.ProfileNamesList)
                .GetRandomElement();
    }
}
