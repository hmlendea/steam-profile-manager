using System.IO;

using NuciExtensions;
using NuciLog.Core;
using NuciWeb.Steam;

using SteamProfileManager.Configuration;
using SteamProfileManager.Logging;

namespace SteamProfileManager.Service
{
    public sealed class ProfileManager(
        IInfoGenerator infoGenerator,
        IFileDownloader fileDownloader,
        ISteamProcessor steamProcessor,
        BotSettings botSettings,
        ILogger logger) : IProfileManager
    {
        readonly IInfoGenerator infoGenerator = infoGenerator;
        readonly IFileDownloader fileDownloader = fileDownloader;
        readonly ISteamProcessor steamProcessor = steamProcessor;
        readonly BotSettings botSettings = botSettings;
        readonly ILogger logger = logger;

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

            string profileName;

            if (!string.IsNullOrWhiteSpace(botSettings.ProfileNamesList))
            {
                profileName = GetRandomEntryFromList(botSettings.ProfileNamesList);
            }
            else
            {
                profileName = infoGenerator.GetRandomProfileName();
            }

            steamProcessor.SetProfileName(profileName);

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

        public void SetRandomProfilePicture()
        {
            logger.Debug(
                MyOperation.SetProfilePicture,
                OperationStatus.Started);

            string profilePictureUrl;

            if (!string.IsNullOrWhiteSpace(botSettings.ProfilePicturesList))
            {
                profilePictureUrl = GetRandomEntryFromList(botSettings.ProfilePicturesList);
            }
            else
            {
                profilePictureUrl = "https://i.imgur.com/rV2RQMO.png";
            }

            string profilePictureFilePath = Path.Join(Directory.GetCurrentDirectory(), "profilePicture.png");

            fileDownloader.Download(profilePictureUrl, profilePictureFilePath);
            steamProcessor.SetProfilePicture(profilePictureFilePath);

            logger.Info(
                MyOperation.SetProfilePicture,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.ProfilePicture, profilePictureUrl));
        }

        private static string GetRandomEntryFromList(string listPath)
            => File
                .ReadAllLines(listPath)
                .GetRandomElement();
    }
}
