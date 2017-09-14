using System;

using SteamKit2;

namespace SteamProfileManager.Models
{
    public class SteamUserId
    {
        SteamID steamId;

        public string SteamId32
        {
            get
            {
                return steamId.ToString();
            }
            set
            {
                steamId = new SteamID(value);
            }
        }

        public string SteamId64
        {
            get
            {
                return steamId.ConvertToUInt64().ToString();
            }
            set
            {
                UInt64 steamId64 = UInt64.Parse(value);
                steamId = new SteamID(steamId64);
            }
        }

        public string AccountId
        {
            get
            {
                return steamId.AccountID.ToString();
            }
            set
            {
                steamId.AccountID = uint.Parse(value);
            }
        }

        public SteamUserId(string steamId32)
        {
            steamId = new SteamID(steamId32);
        }

        public SteamUserId(UInt64 steamId64)
        {
            steamId = new SteamID(steamId64);
        }

        public SteamUserId(SteamID steamId)
        {
            this.steamId = steamId;
        }
    }
}
