using System;

namespace SteamProfileManager
{
    class Program
    {
        static SteamClient client;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Exactly two arguments must be sent to this application (Username and Password)");
            }

            string username = args[0];
            string password = args[1];

            client = new SteamClient();

            RegisterEvents();

            Console.WriteLine("Connecting to Steam...");
            client.LogIn(username, password);

            Console.ReadKey();
        }

        static void RegisterEvents()
        {
            client.Connected += OnClientConnected;
            client.Disconnected += OnClientDisconnected;
            client.LoggedIn += OnClientLoggedIn;
            client.LoggedOut += OnClientLoggedOut;
        }

        static void OnClientConnected(object sender, EventArgs e)
        {
            Console.WriteLine($"Connected to Steam!");
        }

        static void OnClientDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from Steam");
        }

        static void OnClientLoggedIn(object sender, EventArgs e)
        {
            Console.WriteLine($"Logged in to Steam! (SteamID: {client.AccountId})");
        }

        static void OnClientLoggedOut(object sender, EventArgs e)
        {
            Console.WriteLine($"Logged out of Steam: {client.AccountId}");
        }
    }
}