using NuciLog.Core;

namespace SteamProfileManager.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name)
            : base(name)
        {
            
        }

        public static Operation LogIn => new MyOperation(nameof(LogIn));

        public static Operation SetProfileName => new MyOperation(nameof(SetProfileName));
    }
}