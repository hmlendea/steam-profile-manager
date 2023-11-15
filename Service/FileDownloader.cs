using System.Net;

namespace SteamProfileManager.Service
{
    public sealed class FileDownloader : IFileDownloader
    {
        public void Download(string url, string outputPath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, outputPath);
            }
        }
    }
}
