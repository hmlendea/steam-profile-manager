using System;
using System.Linq;

using SteamProfileManager.BusinessLogic.SteamManagers.Interfaces;
using SteamProfileManager.Web;

namespace SteamProfileManager.BusinessLogic.SteamManagers
{
    public class ProfileManager : IProfileManager
    {
        static SteamClient client;

        public ProfileManager()
        {
            client = new SteamClient();

            RegisterEvents();
        }

        public void LogIn(string username, string password)
        {
            client.LogIn(username, password);
        }

        public void LogIn(string username, string password, string authCode)
        {
            client.LogIn(username, password, authCode);
        }

        public void LogIn(string username, string password, string webSteamLogin, string webSessionId)
        {
            client.LogIn(username, password, webSteamLogin, webSessionId);
        }

        public void LogIn(string username, string password, string authCode, string webSteamLogin, string webSessionId)
        {
            client.LogIn(username, password, authCode, webSteamLogin, webSessionId);
        }

        public void HandleProfileEvents()
        {
            while (client.IsConnected)
            {
                // TODO: Handle events here
            }
        }

        void RegisterEvents()
        {
            client.Connected += OnClientConnected;
            client.Disconnected += OnClientDisconnected;
            client.LoggedIn += OnClientLoggedIn;
            client.LoggedOut += OnClientLoggedOut;
            client.CommunityLoaded += OnClientCommunityLoaded;
            client.ClientFullyLoaded += OnClientFullyLoaded;
        }

        void OnClientConnected(object sender, EventArgs e)
        {
            Console.WriteLine($"Connected to Steam");
        }

        void OnClientDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from Steam");
        }

        void OnClientLoggedIn(object sender, EventArgs e)
        {
            Console.WriteLine($"Logged in to Steam. (SteamID: {client.CurrentUser.SteamId.SteamId64})");
        }

        void OnClientLoggedOut(object sender, EventArgs e)
        {
            Console.WriteLine($"Logged out of Steam: {client.CurrentUser.SteamId.SteamId64}");
        }

        void OnClientCommunityLoaded(object sender, EventArgs e)
        {
            int onlineFriendsCount = client.Friends.Count(f => f.IsOnline);

            Console.WriteLine($"We have {client.Friends.Count} friends ({onlineFriendsCount} online)");
        }

        void OnClientFullyLoaded(object sender, EventArgs e)
        {
            Console.WriteLine($"Client fully loaded");
        }
    }
}
