using System;

namespace Plugins.DownloadManager.Interfaces
{
    public interface IDownloadPaths
    {
        string DownloadDirectory { get; }
        string GetLocalPath(string url);
        string GetLocalPath(Uri uri);
    }
}
