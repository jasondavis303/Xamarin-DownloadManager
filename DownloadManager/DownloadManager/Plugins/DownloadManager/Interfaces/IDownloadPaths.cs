using System.Collections.Generic;

namespace Plugins.DownloadManager.Interfaces
{
    public interface IDownloadPaths
    {
        string TempDirectory { get; }
        string DownloadDirectory { get; }
        string GetTempPath(string url);
        string GetLocalPath(string url);

        /// <summary>
        /// Moves a downloaded file (from the url) from the temp location to the permanent one. Call once IDownloadFile.Status == DownloadFileStatus.COMPLETED
        /// </summary>
        void MoveFile(string url);
    }
}
