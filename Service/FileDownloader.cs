using System.Net;

namespace SteamProfileManager.Service
{
    public sealed class FileDownloader : IFileDownloader
    {
        public void Download(string url, string outputPath)
            => new WebClient().DownloadFile(url, outputPath);
    }
}
