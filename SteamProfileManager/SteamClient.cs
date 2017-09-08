using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography;

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
        protected string AuthenticationCode { get; private set; }

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

        public void LogIn(string username, string password, string authCode)
        {
            AuthenticationCode = authCode;

            LogIn(username, password);
        }

        public void LogOff()
        {
            user.LogOff();
        }

        byte[] GetSentryHash()
        {
            byte[] sentryHash = null;

            if (File.Exists("sentry.bin"))
            {
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = SK.CryptoHelper.SHAHash(sentryFile);
            }

            return sentryHash;
        }

        void OnConnected(SK.SteamClient.ConnectedCallback callback)
        {
            SK.SteamUser.LogOnDetails loginDetails = new SK.SteamUser.LogOnDetails
            {
                Username = Username,
                Password = Password
            };

            if (!string.IsNullOrWhiteSpace(AuthenticationCode))
            {
                loginDetails.AuthCode = AuthenticationCode;
                loginDetails.TwoFactorCode = AuthenticationCode;
                loginDetails.SentryFileHash = GetSentryHash();
            }

            user.LogOn(loginDetails);

            Connected?.Invoke(this, null);
        }

        void OnDisconnected(SK.SteamClient.DisconnectedCallback callback)
        {
            IsConnected = false;
            Disconnected?.Invoke(this, null);
        }

        void OnLoggedOn(SK.SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == SK.EResult.AccountLogonDenied;
            bool is2FA = callback.Result == SK.EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                Console.WriteLine("This account is SteamGuard protected!");
            }

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

        void OnMachineAuth(SK.SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            int fileSize;
            byte[] sentryHash;

            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = SHA1.Create())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            user.SendMachineAuthResponse(new SK.SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,
                FileName = callback.FileName,
                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,
                Result = SK.EResult.OK,
                LastError = 0,
                OneTimePassword = callback.OneTimePassword,
                SentryFileHash = sentryHash,
            });
        }
    }
}
