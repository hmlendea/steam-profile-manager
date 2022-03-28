using NuciExtensions;

namespace SteamProfileManager.Service
{
    public sealed class InfoGenerator : IInfoGenerator
    {
        static string LowerCaseCharacters => "abcdefghijklmnopqrstuvwxyz";
        static string UpperCaseCharacters => "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static string NumericCharacters => "0123456789";
        static string AllCharacters => $"{LowerCaseCharacters}{UpperCaseCharacters}{NumericCharacters}";

        public string GetRandomProfileName()
        {
            string username = string.Empty;

            for (int i = 0; i < 12; i++)
            {
                username += AllCharacters.GetRandomElement();
            }

            return username;
        }
    }
}
