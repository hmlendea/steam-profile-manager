namespace SteamProfileManager.Service
{
    public interface IProfileManager
    {
        void LogIn();

        void SetRandomProfileName();

        void SetRandomProfileIdentifier();

        void SetRandomProfilePicture();
    }
}
