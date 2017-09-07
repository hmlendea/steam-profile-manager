using System;
using System.Security.Authentication;

using SK = SteamKit2;

namespace SteamProfileManager
{
    public class SteamClient
    {
        SK.SteamClient client;
        SK.CallbackManager manager;

        SK.SteamUser user;
        SK.SteamFriends community;

        // TODO: Create entities for some of these
        public string AccountUsername { get; private set; }
        protected string AccountPassword { get; private set; }
        public SK.SteamID AccountId { get; private set; }
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

            client = new SK.SteamClient();

            manager = new SK.CallbackManager(client);

            user = client.GetHandler<SK.SteamUser>();
            community = client.GetHandler<SK.SteamFriends>();

            manager.Subscribe<SK.SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SK.SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SK.SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SK.SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SK.SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SK.SteamFriends.FriendsListCallback>(OnFriendsList);
            manager.Subscribe<SK.SteamFriends.PersonaStateCallback>(OnPersonaState);
            manager.Subscribe<SK.SteamFriends.FriendAddedCallback>(OnFriendAdded);

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

        void OnConnected(SK.SteamClient.ConnectedCallback callback)
        {
            user.LogOn(new SK.SteamUser.LogOnDetails
            {
                Username = AccountUsername,
                Password = AccountPassword,
            });

            Connected?.Invoke(this, null);
        }

        void OnDisconnected(SK.SteamClient.DisconnectedCallback callback)
        {
            IsRunning = false;
            Disconnected?.Invoke(this, null);
        }

        void OnLoggedOn(SK.SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != SK.EResult.OK)
            {
                IsRunning = false;

                if (callback.Result == SK.EResult.AccountLogonDenied)
                {
                    throw new AuthenticationException("Unable to logon to Steam: This account is SteamGuard protected.");
                }

                throw new AuthenticationException($"Unable to logon to Steam: {callback.Result} / {callback.ExtendedResult}");
            }

            AccountId = user.SteamID;

            LoggedIn?.Invoke(this, null);
        }

        void OnLoggedOff(SK.SteamUser.LoggedOffCallback callback)
        {
            LoggedOut?.Invoke(this, null);
        }

        void OnAccountInfo(SK.SteamUser.AccountInfoCallback callback)
        {
            community.SetPersonaState(SK.EPersonaState.Online);
        }

        void OnFriendsList(SK.SteamFriends.FriendsListCallback callback)
        {
            Console.WriteLine($"We have {callback.FriendList.Count} friends");

            foreach (SK.SteamFriends.FriendsListCallback.Friend friend in callback.FriendList)
            {
                string friendName = community.GetFriendPersonaName(friend.SteamID);

                if (friend.Relationship == SK.EFriendRelationship.RequestRecipient)
                {
                    Console.WriteLine($"Pending friend request: {friendName}");
                }
            }
        }

        void OnFriendAdded(SK.SteamFriends.FriendAddedCallback callback)
        {
            Console.WriteLine($"New friend: {callback.PersonaName}");
        }

        void OnPersonaState(SK.SteamFriends.PersonaStateCallback callback)
        {
            if (callback.FriendID == AccountId)
            {
                UserAlias = community.GetPersonaName();
            }
        }
    }
}
