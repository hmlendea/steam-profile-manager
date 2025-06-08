using NuciLog.Core;

namespace SteamProfileManager.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        MyLogInfoKey(string name) : base(name) { }

        public static LogInfoKey Username => new MyLogInfoKey(nameof(Username));

        public static LogInfoKey ProfileName => new MyLogInfoKey(nameof(ProfileName));

        public static LogInfoKey ProfileIdentifier => new MyLogInfoKey(nameof(ProfileIdentifier));

        public static LogInfoKey ProfilePicture => new MyLogInfoKey(nameof(ProfilePicture));
    }
}