using Plugins.DownloadManager.Interfaces;
using System;
using System.IO;

namespace Plugins.DownloadManager.iOS
{
    public class DownloadPaths : IDownloadPaths
    {
        public static readonly DownloadPaths Current = new DownloadPaths();

        private DownloadPaths() { }

        public string DownloadDirectory
        {
            get
            {
                string ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "Caches", "Downloaded");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string TempDirectory
        {
            get
            {
                string ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "tmp");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string GetTempPath(string url) => GetTempPath(new Uri(url));

        public string GetTempPath(Uri uri)
        {
            string ret = Path.Combine(TempDirectory, uri.GetLocalHostPath());
            Directory.CreateDirectory(Path.GetFileName(ret));
            return ret;
        }

        public string GetLocalPath(string url) => GetLocalPath(new Uri(url));

        public string GetLocalPath(Uri uri)
        {
            string ret = Path.Combine(DownloadDirectory, uri.GetLocalHostPath());
            Directory.CreateDirectory(Path.GetFileName(ret));
            return ret;
        }

        public void MoveFile(string url) => MoveFile(new Uri(url));

        public void MoveFile(Uri uri)
        {
            string local = GetLocalPath(uri);
            if (File.Exists(local))
                File.Delete(local);

            File.Move(GetTempPath(uri), local);
        }
    }
}