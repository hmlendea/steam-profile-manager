using System;
using System.Security.Authentication;

using SteamKit2;

namespace SteamProfileManager
{
    class Program
    {
        static SteamClient client;
        static CallbackManager manager;
        static SteamUser user;

        static string username;
        static string password;

        static bool isRunning;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Exactly two arguments must be sent to this application (Username and Password)");
            }

            username = args[0];
            password = args[1];

            client = new SteamClient();

            manager = new CallbackManager(client);

            user = client.GetHandler<SteamUser>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            isRunning = true;

            Console.WriteLine("Connecting to Steam...");
            
            client.Connect();
            
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Console.WriteLine($"Connected to Steam! Logging in '{username}'...");

            user.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");

            isRunning = false;
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                isRunning = false;

                if (callback.Result == EResult.AccountLogonDenied)
                {
                    throw new AuthenticationException("Unable to logon to Steam: This account is SteamGuard protected.");
                }

                throw new AuthenticationException($"Unable to logon to Steam: {callback.Result} / {callback.ExtendedResult}");
            }

            Console.WriteLine("Successfully logged on!");

            // TODO: Do stuff here
            
            user.LogOff();
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine($"Logged off of Steam: {callback.Result}");
        }
    }
}