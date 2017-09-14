using System;

using SteamProfileManager.BusinessLogic.SteamManagers;
using SteamProfileManager.BusinessLogic.SteamManagers.Interfaces;

namespace SteamProfileManager
{
    class Program
    {
        static IProfileManager profileManager;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("At least two arguments must be sent to this application (Username and Password)");
            }

            string username = args[0];
            string password = args[1];
            string authCode = null;
            string webSteamLogin = null;
            string webSessionId = null;

            if (args.Length == 3)
            {
                authCode = args[2];
            }

            if (args.Length == 4)
            {
                webSteamLogin = args[2];
                webSessionId = args[3];
            }
            else if (args.Length == 5)
            {
                webSteamLogin = args[3];
                webSessionId = args[4];
            }

            profileManager = new ProfileManager();
            
            Console.WriteLine("Connecting to Steam");

            if (!string.IsNullOrEmpty(authCode) && string.IsNullOrEmpty(webSteamLogin))
            {
                profileManager.LogIn(username, password, authCode);
            }
            else if (string.IsNullOrEmpty(authCode) && !string.IsNullOrEmpty(webSteamLogin))
            {
                profileManager.LogIn(username, password, webSteamLogin, webSessionId);
            }
            else if (!string.IsNullOrEmpty(authCode) && !string.IsNullOrEmpty(webSteamLogin))
            {
                profileManager.LogIn(username, password, authCode, webSteamLogin, webSessionId);
            }
            else
            {
                profileManager.LogIn(username, password);
            }

            profileManager.HandleProfileEvents();

            Console.ReadKey();
        }
    }
}