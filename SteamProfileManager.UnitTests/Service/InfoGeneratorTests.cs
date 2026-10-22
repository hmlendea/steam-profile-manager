using NUnit.Framework;

using SteamProfileManager.Service;

namespace SteamProfileManager.UnitTests.Service
{
    [TestFixture]
    public sealed class InfoGeneratorTests
    {
        [Test]
        public void GivenAnInfoGenerator_WhenGeneratingAProfileName_ThenTwelveAlphanumericCharactersAreReturned()
            => Assert.That(
                new InfoGenerator().GetRandomProfileName(),
                Does.Match("\\A[a-zA-Z0-9]{12}\\z"));

        [Test]
        public void GivenAnInfoGenerator_WhenGeneratingAnIdentifier_ThenEightAlphanumericCharactersAreReturned()
            => Assert.That(
                new InfoGenerator().GetRandomProfileIdentifier(),
                Does.Match("\\A[a-zA-Z0-9]{8}\\z"));
    }
}