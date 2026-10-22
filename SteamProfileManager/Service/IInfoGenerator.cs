namespace SteamProfileManager.Service
{
    public interface IInfoGenerator
    {
        string GetRandomProfileName();

        string GetRandomProfileIdentifier();
    }
}
