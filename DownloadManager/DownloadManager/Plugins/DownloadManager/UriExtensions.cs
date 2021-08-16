using System;

namespace Plugins.DownloadManager
{
    public static class UriExtensions
    {
        public static string GetLocalHostPath(this Uri @this) => $"{@this.DnsSafeHost}{@this.LocalPath}";
    }
}
