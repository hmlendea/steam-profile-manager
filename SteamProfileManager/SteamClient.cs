using System;
using System.Security.Authentication;

using SteamProfileManager.Models;

using SK = SteamKit2;

namespace SteamProfileManager
{
    public class SteamClient
    {
        SK.SteamClient client;
        SK.CallbackManager manager;

        SK.SteamUser user;
        SK.SteamFriends community;
        
        public SteamUser CurrentUser { get; private set; }

        public string Username { get; private set; }
        protected string Password { get; private set; }
        public bool IsConnected { get; private set; }

        // TODO: Use custom event handlers
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public void LogIn(string username, string password)
        {
            Username = username;
            Password = password;

            client = new SK.SteamClient();
            CurrentUser = new SteamUser();

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

            IsConnected = true;
            client.Connect();

            while (IsConnected)
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
                Username = Username,
                Password = Password,
            });

            Connected?.Invoke(this, null);
        }

        void OnDisconnected(SK.SteamClient.DisconnectedCallback callback)
        {
            IsConnected = false;
            Disconnected?.Invoke(this, null);
        }

        void OnLoggedOn(SK.SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != SK.EResult.OK)
            {
                IsConnected = false;

                if (callback.Result == SK.EResult.AccountLogonDenied)
                {
                    throw new AuthenticationException("Unable to logon to Steam: This account is SteamGuard protected.");
                }

                throw new AuthenticationException($"Unable to logon to Steam: {callback.Result} / {callback.ExtendedResult}");
            }
            
            CurrentUser.AccountId = (int)user.SteamID.AccountID;

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
            if (callback.FriendID.AccountID == CurrentUser.AccountId)
            {
                CurrentUser.Name = community.GetPersonaName();
            }
        }
    }
}
