namespace SteamProfileManager.BusinessLogic.SteamManagers.Interfaces
{
    public interface IProfileManager
    {
        void LogIn(string username, string password);

        void LogIn(string username, string password, string authCode);

        void LogIn(string username, string password, string webSteamLogin, string webSessionId);

        void LogIn(string username, string password, string authCode, string webSteamLogin, string webSessionId);

        void HandleProfileEvents();
    }
}
