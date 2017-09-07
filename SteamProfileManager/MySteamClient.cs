using System;
using System.Security.Authentication;

using SteamKit2;

using SteamFriend = SteamKit2.SteamFriends.FriendsListCallback.Friend;

namespace SteamProfileManager
{
    // TODO: Choose a proper name
    public class MySteamClient
    {
        SteamClient client;
        CallbackManager manager;

        SteamUser user;
        SteamFriends community;

        // TODO: Create entities for some of these
        public string AccountUsername { get; private set; }
        protected string AccountPassword { get; private set; }
        public SteamID AccountId { get; private set; }
        public string UserAlias { get; private set; }

        public bool IsRunning { get; private set; }

        // TODO: Use custom event handlers
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public void LogIn(string username, string password)
        {
            AccountUsername = username;
            AccountPassword = password;

            client = new SteamClient();

            manager = new CallbackManager(client);

            user = client.GetHandler<SteamUser>();
            community = client.GetHandler<SteamFriends>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);
            manager.Subscribe<SteamFriends.PersonaStateCallback>(OnPersonaState);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendAdded);

            IsRunning = true;
            client.Connect();

            while (IsRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        public void LogOff()
        {
            user.LogOff();
        }

        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            user.LogOn(new SteamUser.LogOnDetails
            {
                Username = AccountUsername,
                Password = AccountPassword,
            });

            Connected?.Invoke(this, null);
        }

        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            IsRunning = false;
            Disconnected?.Invoke(this, null);
        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                IsRunning = false;

                if (callback.Result == EResult.AccountLogonDenied)
                {
                    throw new AuthenticationException("Unable to logon to Steam: This account is SteamGuard protected.");
                }

                throw new AuthenticationException($"Unable to logon to Steam: {callback.Result} / {callback.ExtendedResult}");
            }

            AccountId = user.SteamID;

            LoggedIn?.Invoke(this, null);
        }

        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            LoggedOut?.Invoke(this, null);
        }

        void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            community.SetPersonaState(EPersonaState.Online);
        }

        void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            Console.WriteLine($"We have {callback.FriendList.Count} friends");

            foreach (SteamFriend friend in callback.FriendList)
            {
                string friendName = community.GetFriendPersonaName(friend.SteamID);

                if (friend.Relationship == EFriendRelationship.RequestRecipient)
                {
                    Console.WriteLine($"Pending friend request: {friendName}");
                }
            }
        }

        void OnFriendAdded(SteamFriends.FriendAddedCallback callback)
        {
            Console.WriteLine($"New friend: {callback.PersonaName}");
        }

        void OnPersonaState(SteamFriends.PersonaStateCallback callback)
        {
            if (callback.FriendID == AccountId)
            {
                UserAlias = community.GetPersonaName();
            }
        }
    }
}
