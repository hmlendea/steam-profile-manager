using System.IO;

using Moq;

using NUnit.Framework;

using NuciLog.Core;

using NuciWeb.Steam;
using NuciWeb.Steam.Models;

using SteamProfileManager.Configuration;
using SteamProfileManager.Service;

namespace SteamProfileManager.UnitTests.Service
{
    [TestFixture]
    public sealed class ProfileManagerTests
    {
        private Mock<IInfoGenerator> infoGenerator = null!;
        private Mock<IFileDownloader> fileDownloader = null!;
        private Mock<ISteamProcessor> steamProcessor = null!;
        private BotSettings botSettings = null!;
        private ProfileManager profileManager = null!;

        [SetUp]
        public void SetUp()
        {
            infoGenerator = new(MockBehavior.Strict);
            fileDownloader = new(MockBehavior.Strict);
            steamProcessor = new(MockBehavior.Strict);
            botSettings = new()
            {
                SteamAccount = new SteamAccount(),
                ProfileNamesList = string.Empty,
                ProfilePicturesList = string.Empty
            };
            profileManager = new(
                infoGenerator.Object,
                fileDownloader.Object,
                steamProcessor.Object,
                botSettings,
                Mock.Of<ILogger>());
        }

        [Test]
        public void GivenAnAccount_WhenLoggingIn_ThenCookiesAreRejectedAfterAuthentication()
        {
            MockSequence sequence = new();
            steamProcessor.InSequence(sequence)
                .Setup(processor => processor.LogIn(botSettings.SteamAccount));
            steamProcessor.InSequence(sequence).Setup(processor => processor.RejectCookies());

            profileManager.LogIn();

            steamProcessor.Verify(processor => processor.LogIn(botSettings.SteamAccount), Times.Once);
            steamProcessor.Verify(processor => processor.RejectCookies(), Times.Once);
            steamProcessor.VerifyNoOtherCalls();
            infoGenerator.VerifyNoOtherCalls();
            fileDownloader.VerifyNoOtherCalls();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public void GivenNoNamesList_WhenSettingAProfileName_ThenTheGeneratedNameIsUsed(string? listPath)
        {
            botSettings.ProfileNamesList = listPath!;
            infoGenerator.Setup(generator => generator.GetRandomProfileName()).Returns("Angetenar");
            steamProcessor.Setup(processor => processor.SetProfileName("Angetenar"));

            profileManager.SetRandomProfileName();

            infoGenerator.Verify(generator => generator.GetRandomProfileName(), Times.Once);
            steamProcessor.Verify(processor => processor.SetProfileName("Angetenar"), Times.Once);
            infoGenerator.VerifyNoOtherCalls();
            steamProcessor.VerifyNoOtherCalls();
            fileDownloader.VerifyNoOtherCalls();
        }

        [TestCase("Angetenar")]
        [TestCase("DummyUser")]
        [TestCase("solaire_of_astora")]
        public void GivenAGeneratedIdentifier_WhenSettingAnIdentifier_ThenItIsPassedUnchanged(string identifier)
        {
            infoGenerator.Setup(generator => generator.GetRandomProfileIdentifier()).Returns(identifier);
            steamProcessor.Setup(processor => processor.SetProfileIdentifier(identifier));

            profileManager.SetRandomProfileIdentifier();

            infoGenerator.Verify(generator => generator.GetRandomProfileIdentifier(), Times.Once);
            steamProcessor.Verify(processor => processor.SetProfileIdentifier(identifier), Times.Once);
            infoGenerator.VerifyNoOtherCalls();
            steamProcessor.VerifyNoOtherCalls();
            fileDownloader.VerifyNoOtherCalls();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public void GivenNoPicturesList_WhenSettingAPicture_ThenTheDefaultIsDownloadedPriorToUpload(
            string? listPath)
        {
            botSettings.ProfilePicturesList = listPath!;
            string picturePath = Path.Join(Directory.GetCurrentDirectory(), "profilePicture.png");
            MockSequence sequence = new();
            fileDownloader.InSequence(sequence)
                .Setup(downloader => downloader.Download("https://i.imgur.com/rV2RQMO.png", picturePath));
            steamProcessor.InSequence(sequence)
                .Setup(processor => processor.SetProfilePicture(picturePath));

            profileManager.SetRandomProfilePicture();

            fileDownloader.Verify(
                downloader => downloader.Download("https://i.imgur.com/rV2RQMO.png", picturePath),
                Times.Once);
            steamProcessor.Verify(processor => processor.SetProfilePicture(picturePath), Times.Once);
            fileDownloader.VerifyNoOtherCalls();
            steamProcessor.VerifyNoOtherCalls();
            infoGenerator.VerifyNoOtherCalls();
        }
    }
}