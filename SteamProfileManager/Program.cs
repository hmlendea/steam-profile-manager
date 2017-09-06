using System;
using System.Security.Authentication;

using SteamKit2;

using SteamFriend = SteamKit2.SteamFriends.FriendsListCallback.Friend;

namespace SteamProfileManager
{
    class Program
    {
        static SteamClient client;
        static CallbackManager manager;

        static SteamUser user;
        static SteamFriends friends;

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
            friends = client.GetHandler<SteamFriends>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

            isRunning = true;

            Console.WriteLine("Connecting to Steam...");

            client.Connect();

            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            Console.ReadKey();
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

            Console.WriteLine($"Successfully logged on! (SteamID: {user.SteamID.AccountID})");

            // TODO: Do stuff here
            
            user.LogOff();
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine($"Logged off of Steam: {callback.Result}");
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            friends.SetPersonaState(EPersonaState.Online);
        }

        static void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            Console.WriteLine($"We have {callback.FriendList.Count} friends");
            
            foreach (SteamFriend friend in callback.FriendList)
            {
                string friendName = friends.GetFriendPersonaName(friend.SteamID);

                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    Console.WriteLine($"Pending friend request: {friendName}");
                }
            }
        }

        static void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            Console.WriteLine($"New friend: {callback.PersonaName}");
        }

        static void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            Console.WriteLine($"Friend name changed: {callback.FriendID}={callback.Name}");
        }
    }
}