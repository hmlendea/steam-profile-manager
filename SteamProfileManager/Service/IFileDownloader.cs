namespace SteamProfileManager.Service
{
    public interface IFileDownloader
    {
        void Download(string url, string outputPath);
    }
}
