using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;

using SteamProfileManager.Models;

using SK = SteamKit2;

using SteamFriend = SteamKit2.SteamFriends.FriendsListCallback.Friend;

namespace SteamProfileManager
{
    public class SteamClient
    {
        SK.SteamClient client;
        SK.CallbackManager manager;

        SK.SteamUser steamUser;
        SK.SteamFriends community;

        public SteamUser CurrentUser { get; private set; }

        public string Username { get; private set; }
        protected string Password { get; private set; }
        protected string AuthenticationCode { get; private set; }

        public List<SteamUser> Friends { get; private set; }

        public bool IsConnected { get; private set; }

        // TODO: Use custom event handlers
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public SteamClient()
        {
            Friends = new List<SteamUser>();
        }

        public void LogIn(string username, string password)
        {
            Username = username;
            Password = password;

            client = new SK.SteamClient();
            CurrentUser = new SteamUser();

            manager = new SK.CallbackManager(client);

            steamUser = client.GetHandler<SK.SteamUser>();
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
            steamUser.LogOff();
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

        bool GetScammerStatus(string steamId)
        {
            string result = string.Empty;
            string url = $"http://steamrep.com/id2rep.php?steamID32={steamId}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result.Contains("SCAMMER");
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

            steamUser.LogOn(loginDetails);

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

            CurrentUser.SteamId = steamUser.SteamID.ToString();
            CurrentUser.AccountId = (int)steamUser.SteamID.AccountID;
            CurrentUser.IsScammer = GetScammerStatus(CurrentUser.SteamId);

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
            foreach (SteamFriend friend in callback.FriendList)
            {
                SteamUser friendUser = new SteamUser
                {
                    SteamId = friend.SteamID.ToString(),
                    AccountId = (int)friend.SteamID.AccountID,
                    Name = community.GetFriendPersonaName(friend.SteamID),
                    IsOnline = community.GetFriendPersonaState(friend.SteamID) != 0,
                    IsScammer = GetScammerStatus(friend.SteamID.ToString())
                };

                if (friendUser.Name == "[unknown]")
                {
                    continue;
                }

                Friends.Add(friendUser);

                if (friend.Relationship == SK.EFriendRelationship.RequestRecipient)
                {
                    Console.WriteLine($"Pending friend request: {friendUser.Name}");
                }
            }

            int onlineFriendsCount = Friends.Count(f => f.IsOnline);

            Console.WriteLine($"We have {Friends.Count} friends ({onlineFriendsCount} online)");
        }

        void OnFriendAdded(SK.SteamFriends.FriendAddedCallback callback)
        {
            Console.WriteLine($"New friend: {callback.PersonaName}");
        }

        void OnPersonaState(SK.SteamFriends.PersonaStateCallback callback)
        {
            SteamUser user;

            if (callback.FriendID.ToString() == CurrentUser.SteamId)
            {
                user = CurrentUser;
            }
            else
            {
                user = Friends.FirstOrDefault(f => f.SteamId == callback.FriendID.ToString());
            }

            if (user == null)
            {
                return;
            }

            bool newStatus = (callback.State != 0);

            user.Name = community.GetPersonaName();
            user.LastOnlineDate = new DateTime(Math.Max(callback.LastLogOn.Ticks, callback.LastLogOff.Ticks));

            if (user.IsOnline != newStatus)
            {
                if (newStatus == true)
                {
                    Console.WriteLine($"User '{user.Name}' is now online");
                }
                else
                {
                    Console.WriteLine($"User '{user.Name}' is now offline");
                }
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
            steamUser.SendMachineAuthResponse(new SK.SteamUser.MachineAuthDetails
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
